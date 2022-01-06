// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Analyzes and reports on tracking session statistics.
/// Generates reports on distance traveled, speed, duration, and other metrics.
/// </summary>
public class SessionAnalyticsReporter
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public SessionAnalyticsReporter(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Generates a comprehensive analytics report for a vehicle.
    /// </summary>
    public async Task GenerateVehicleAnalyticsReport(string vehicleId)
    {
        Console.WriteLine("=== Session Analytics Reporter ===\n");
        Console.WriteLine($"Generating analytics for Vehicle: {vehicleId}\n");

        try
        {
            var sessions = await GetVehicleSessions(vehicleId);
            if (sessions.Count == 0)
            {
                Console.WriteLine("No tracking sessions found for this vehicle.");
                return;
            }

            var analytics = CalculateAnalytics(sessions);
            DisplayAnalyticsReport(vehicleId, analytics, sessions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all tracking sessions for a vehicle.
    /// </summary>
    private async Task<List<SessionData>> GetVehicleSessions(string vehicleId)
    {
        var sessions = new List<SessionData>();

        // In a real scenario, you'd call an API endpoint to get sessions
        // For this example, we're generating mock data
        var random = new Random();

        for (int i = 0; i < 5; i++)
        {
            var startTime = DateTime.UtcNow.AddDays(-i);
            var duration = random.Next(60, 480); // 1 to 8 hours
            var totalDistance = random.Next(10, 200); // 10-200 km
            var maxSpeed = 50 + random.Next(50); // 50-100 km/h
            var avgSpeed = (totalDistance / duration) * 60; // km/h

            sessions.Add(new SessionData
            {
                Id = Guid.NewGuid().ToString(),
                VehicleId = vehicleId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(duration),
                TotalDistance = totalDistance,
                AverageSpeed = avgSpeed,
                MaxSpeed = maxSpeed,
                LocationCount = random.Next(50, 500),
                Status = "Completed"
            });
        }

        return sessions;
    }

    /// <summary>
    /// Calculates aggregate analytics from multiple sessions.
    /// </summary>
    private AnalyticsResult CalculateAnalytics(List<SessionData> sessions)
    {
        var result = new AnalyticsResult();

        foreach (var session in sessions)
        {
            result.TotalDistance += session.TotalDistance;
            result.TotalDuration += (session.EndTime - session.StartTime).TotalHours;
            result.TotalLocations += session.LocationCount;
            result.SessionCount++;

            if (session.MaxSpeed > result.MaxSpeed)
                result.MaxSpeed = session.MaxSpeed;

            if (session.AverageSpeed > 0)
                result.AverageSpeed = (result.AverageSpeed * (result.SessionCount - 1) + session.AverageSpeed) / result.SessionCount;
        }

        result.AverageDurationPerSession = result.TotalDuration / result.SessionCount;
        result.AverageDistancePerSession = result.TotalDistance / result.SessionCount;

        return result;
    }

    /// <summary>
    /// Displays the analytics report in a formatted manner.
    /// </summary>
    private void DisplayAnalyticsReport(string vehicleId, AnalyticsResult analytics, List<SessionData> sessions)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              VEHICLE ANALYTICS REPORT                          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine($"Vehicle ID: {vehicleId}\n");

        Console.WriteLine("SUMMARY STATISTICS");
        Console.WriteLine("──────────────────────────────────────────────────────────────");
        Console.WriteLine($"  Total Sessions:              {analytics.SessionCount}");
        Console.WriteLine($"  Total Distance:             {analytics.TotalDistance:F2} km");
        Console.WriteLine($"  Total Duration:             {analytics.TotalDuration:F2} hours");
        Console.WriteLine($"  Total Locations Recorded:   {analytics.TotalLocations}");

        Console.WriteLine("\nPERFORMANCE METRICS");
        Console.WriteLine("──────────────────────────────────────────────────────────────");
        Console.WriteLine($"  Average Speed:              {analytics.AverageSpeed:F2} km/h");
        Console.WriteLine($"  Maximum Speed:              {analytics.MaxSpeed:F2} km/h");
        Console.WriteLine($"  Avg Distance per Session:   {analytics.AverageDistancePerSession:F2} km");
        Console.WriteLine($"  Avg Duration per Session:   {analytics.AverageDurationPerSession:F2} hours");

        Console.WriteLine("\nRECENT SESSIONS");
        Console.WriteLine("──────────────────────────────────────────────────────────────");

        for (int i = 0; i < Math.Min(5, sessions.Count); i++)
        {
            var session = sessions[i];
            var duration = (session.EndTime - session.StartTime).TotalMinutes;
            var avgSpeed = duration > 0 ? (session.TotalDistance / duration * 60) : 0;

            Console.WriteLine($"\n  Session {i + 1}");
            Console.WriteLine($"    Date:           {session.StartTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"    Duration:       {duration:F0} minutes");
            Console.WriteLine($"    Distance:       {session.TotalDistance:F2} km");
            Console.WriteLine($"    Avg Speed:      {avgSpeed:F2} km/h");
            Console.WriteLine($"    Max Speed:      {session.MaxSpeed:F2} km/h");
            Console.WriteLine($"    Locations:      {session.LocationCount}");
        }

        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Report Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(52) + "║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        GenerateRecommendations(analytics);
    }

    /// <summary>
    /// Generates recommendations based on analytics data.
    /// </summary>
    private void GenerateRecommendations(AnalyticsResult analytics)
    {
        Console.WriteLine("RECOMMENDATIONS");
        Console.WriteLine("──────────────────────────────────────────────────────────────");

        if (analytics.MaxSpeed > 90)
        {
            Console.WriteLine("  ⚠ High speeding detected - Consider driver training");
        }
        else
        {
            Console.WriteLine("  ✓ Speed compliance within normal ranges");
        }

        if (analytics.AverageDistancePerSession > 150)
        {
            Console.WriteLine("  ⚠ Long average distances - Monitor fuel consumption");
        }
        else
        {
            Console.WriteLine("  ✓ Distance metrics appear normal");
        }

        if (analytics.SessionCount < 3)
        {
            Console.WriteLine("  ℹ Limited session data - Gather more data for better insights");
        }
        else
        {
            Console.WriteLine("  ✓ Sufficient data for analysis");
        }

        Console.WriteLine("\n");
    }

    /// <summary>
    /// Exports analytics to CSV format.
    /// </summary>
    public async Task ExportAnalyticsToCsv(string vehicleId, string outputPath)
    {
        Console.WriteLine($"Exporting analytics to: {outputPath}");

        var sessions = await GetVehicleSessions(vehicleId);

        using (var writer = new StreamWriter(outputPath))
        {
            writer.WriteLine("Date,Duration_Minutes,Distance_KM,Avg_Speed,Max_Speed,Locations");

            foreach (var session in sessions)
            {
                var duration = (session.EndTime - session.StartTime).TotalMinutes;
                var avgSpeed = duration > 0 ? (session.TotalDistance / duration * 60) : 0;

                writer.WriteLine(
                    $"{session.StartTime:yyyy-MM-dd},{duration:F0},{session.TotalDistance:F2},{avgSpeed:F2},{session.MaxSpeed:F2},{session.LocationCount}"
                );
            }
        }

        Console.WriteLine("✓ Export completed");
    }

    /// <summary>
    /// Runs the analytics reporter example.
    /// </summary>
    public async Task RunExample(string vehicleId)
    {
        await GenerateVehicleAnalyticsReport(vehicleId);
    }

    private class SessionData
    {
        public string Id { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double TotalDistance { get; set; }
        public double AverageSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public int LocationCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class AnalyticsResult
    {
        public int SessionCount { get; set; }
        public double TotalDistance { get; set; }
        public double TotalDuration { get; set; }
        public int TotalLocations { get; set; }
        public double AverageSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public double AverageDurationPerSession { get; set; }
        public double AverageDistancePerSession { get; set; }
    }

    /// <summary>
    /// Main entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var baseUrl = args.Length > 0 ? args[0] : "https://localhost:5001";
        var apiKey = args.Length > 1 ? args[1] : "default-api-key";
        var vehicleId = args.Length > 2 ? args[2] : Guid.NewGuid().ToString();

        var reporter = new SessionAnalyticsReporter(baseUrl, apiKey);
        await reporter.RunExample(vehicleId);
    }
}
