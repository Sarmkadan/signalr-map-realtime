#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.BackgroundJobs;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Background worker that periodically cleans up inactive tracking sessions.
/// Marks abandoned sessions as completed and archives old location data.
/// Runs on a schedule to prevent database bloat from old tracking records.
/// </summary>
public class SessionCleanupWorker : BackgroundService
{
    private readonly ILogger<SessionCleanupWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _executionInterval = TimeSpan.FromHours(1);
    private readonly TimeSpan _sessionInactivityThreshold = TimeSpan.FromHours(24);
    private readonly TimeSpan _locationArchiveThreshold = TimeSpan.FromDays(30);
    private const int ArchiveBatchSize = 5000;

    public SessionCleanupWorker(ILogger<SessionCleanupWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes the cleanup worker on a scheduled interval.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupInactiveSessions(stoppingToken).ConfigureAwait(false);
                await ArchiveOldLocations(stoppingToken).ConfigureAwait(false);

                await Task.Delay(_executionInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Session cleanup worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in session cleanup worker");
                // Continue on error to prevent worker from stopping
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Session cleanup worker stopped");
    }

    /// <summary>
    /// Marks inactive tracking sessions as completed.
    /// A session is considered inactive if no location updates in the last 24 hours.
    /// </summary>
    private async Task CleanupInactiveSessions(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoffTime = DateTime.UtcNow - _sessionInactivityThreshold;
        var now = DateTime.UtcNow;

        // Set-based update: a single UPDATE statement instead of loading every
        // stale session into the change tracker and issuing per-row updates.
        var updated = await dbContext.TrackingSessions
            .Where(s => s.Status == SessionStatus.Active && s.UpdatedAt < cutoffTime)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, SessionStatus.Completed)
                .SetProperty(s => s.UpdatedAt, now), cancellationToken)
            .ConfigureAwait(false);

        if (updated == 0)
        {
            _logger.LogDebug("No inactive sessions found for cleanup");
            return;
        }

        _logger.LogInformation("Cleaned up {Count} inactive sessions", updated);
    }

    /// <summary>
    /// Archives (deletes) old location records to maintain database performance.
    /// Keeps only the last 30 days of location data.
    /// </summary>
    private async Task ArchiveOldLocations(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoffTime = DateTime.UtcNow - _locationArchiveThreshold;

        // Set-based delete in bounded batches. Loading months of GPS rows into
        // the change tracker just to delete them exhausts memory on real data;
        // batching keeps individual DELETE statements (and their locks) small.
        var totalDeleted = 0;
        int deleted;
        do
        {
            deleted = await dbContext.Locations
                .Where(l => l.CreatedAt < cutoffTime)
                .OrderBy(l => l.Id)
                .Take(ArchiveBatchSize)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            totalDeleted += deleted;
        }
        while (deleted == ArchiveBatchSize && !cancellationToken.IsCancellationRequested);

        if (totalDeleted == 0)
        {
            _logger.LogDebug("No old locations found for archival");
            return;
        }

        _logger.LogInformation("Archived {Count} old locations", totalDeleted);
    }
}

/// <summary>
/// Extension methods for registering the session cleanup worker.
/// </summary>
public static class SessionCleanupWorkerExtensions
{
    /// <summary>
    /// Adds the session cleanup background worker to the host builder.
    /// </summary>
    public static IServiceCollection AddSessionCleanupWorker(this IServiceCollection services)
    {
        services.AddHostedService<SessionCleanupWorker>();
        return services;
    }
}
