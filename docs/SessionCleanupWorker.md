# SessionCleanupWorker

Source: `src/SignalRMapRealtime/BackgroundJobs/SessionCleanupWorker.cs`

A hosted background service (`BackgroundService`) that keeps the tracking tables from growing without limit. On every run it does two things:

1. Marks inactive tracking sessions as completed.
2. Deletes location records older than 30 days.

The file also contains `SessionCleanupWorkerExtensions`, which registers the worker in the DI container.

## What it cleans

### Inactive tracking sessions

A session is inactive when its `Status` is `Active` and its `UpdatedAt` is older than 24 hours (UTC). For each match, the worker sets:

- `Status` to `SessionStatus.Completed`
- `UpdatedAt` to the time of the run

This is one set-based `UPDATE` issued with `ExecuteUpdateAsync`. Matching sessions are not loaded into the change tracker. The session's location rows are not changed by this step.

Note: the XML summary says "no location updates in the last 24 hours". The code does not look at location rows. It checks the session's own `UpdatedAt` column.

### Old location records

Location rows with `CreatedAt` older than 30 days (UTC) are **deleted**. The file does not write them anywhere first, so despite the wording "archives" in the comments, nothing is archived.

Deletion runs in batches of up to 5,000 rows. Each batch is an `ExecuteDeleteAsync` call on `Locations`, filtered by `CreatedAt`, ordered by `Id`, and limited with `Take`. The worker keeps issuing batches while the last one was full (5,000 rows deleted) and cancellation has not been requested. A partial batch ends the loop.

## Schedule and triggers

The worker has no external trigger. It runs on its own timer after the host starts.

| Step | Behavior |
|------|----------|
| Startup | Logs `Session cleanup worker started`, then runs one cleanup pass immediately. |
| Steady state | A `PeriodicTimer` with a 1-hour period. After each successful pass, the worker waits for the next tick and then runs again. |
| Failure | Any exception other than cancellation is logged at Error level. The worker waits 30 seconds, then runs a new pass. This retry does not wait for the hourly tick. |
| Shutdown | When the host's stopping token is cancelled, the loop exits and the worker logs `Session cleanup worker stopped`. |

Each pass creates a new DI scope and resolves `ApplicationDbContext` from it. The session update runs first, then the location deletion, in the same scope.

## Configuration

All values are `private static readonly` or `private const` fields. None are read from configuration, and there are no options classes for this worker.

| Name | Value | Meaning |
|------|-------|---------|
| `ExecutionInterval` | 1 hour | Time between passes after the first one. |
| `SessionInactivityThreshold` | 24 hours | Session age (by `UpdatedAt`) before it is marked completed. |
| `LocationArchiveThreshold` | 30 days | Location age (by `CreatedAt`) before it is deleted. |
| `FailureBackoff` | 30 seconds | Delay after a failed pass before the next attempt. |
| `ArchiveBatchSize` | 5,000 | Maximum rows deleted per statement. |

To change any of these, edit the constant and rebuild.

## Logging

All messages go through `ILogger<SessionCleanupWorker>`.

| Level | When | Message |
|-------|------|---------|
| Information | Worker starts | `Session cleanup worker started` |
| Debug | No sessions matched | `No inactive sessions found for cleanup` |
| Information | Sessions updated | `Cleaned up {Count} inactive sessions` |
| Debug | No location rows deleted | `No old locations found for archival` |
| Information | Location rows deleted | `Archived {Count} old locations` |
| Error | A pass threw an exception | `Error in session cleanup worker` |
| Information | Cancellation during a pass or backoff | `Session cleanup worker is stopping` |
| Information | Loop has exited | `Session cleanup worker stopped` |

The `Archived` message reports deleted rows, for the same reason as above.

## Registration

```csharp
services.AddSessionCleanupWorker();
```

`AddSessionCleanupWorker` is an extension on `IServiceCollection`. It calls `AddHostedService<SessionCleanupWorker>()`. This file does not call it. Check `Program.cs` to confirm that the worker is registered at startup.

## Constructor

```csharp
public SessionCleanupWorker(ILogger<SessionCleanupWorker> logger, IServiceProvider serviceProvider)
```

Both arguments must be non-null, otherwise `ArgumentNullException` is thrown. The worker takes `IServiceProvider` instead of `ApplicationDbContext` directly, because a singleton hosted service cannot consume a scoped `DbContext`. A new scope is created for each pass.

## Failure and retry behavior

- Errors inside a pass are caught, so one failure does not stop the worker.
- A failed pass is not partly rolled back by this code. The session update and the location deletes are separate statements, and each one commits on its own. A failure in the location step does not undo the session update.
- The location deletes in a pass are committed batch by batch. If a later batch fails, earlier batches stay deleted, and the next pass continues from what remains.
