# LocationUpdateThrottler

Source: `src/SignalRMapRealtime/Services/LocationUpdateThrottler.cs`

Singleton service that rate-limits location updates per vehicle and buffers accepted updates so they can be broadcast through `LocationHub` as one coalesced message. The file contains a single type, `LocationUpdateThrottler`. It has no interface and implements `IDisposable`.

This document describes only what the source file does. The members it reads from `ThrottleOptions` (`Enabled`, `CoalesceFlushIntervalMilliseconds`, `GetIntervalForAssetType`) are defined elsewhere and are not covered here.

## Public API

```csharp
public class LocationUpdateThrottler : IDisposable
```

| Member | Purpose |
|--------|---------|
| Constructor `(IOptions<ThrottleOptions>, ILogger<LocationUpdateThrottler>, IHubContext<LocationHub>)` | Stores dependencies, computes the flush interval, and starts the background flush loop. Throws `ArgumentNullException` for any null argument. |
| `bool ShouldThrottle(int vehicleId, AssetType assetType)` | Applies the minimum-interval gate. See [Interval gate](#interval-gate-shouldthrottle). |
| `bool AddToBuffer(LocationDto locationDto, AssetType assetType)` | Passes the update through the interval gate and queues it for the next flush. See [Buffering](#buffering-addtobuffer). |
| `Task<int> FlushImmediatelyAsync(int vehicleId)` | Flushes one vehicle's buffer now. Returns the number of items read, or `0`. |
| `void Remove(int vehicleId)` | Drops all state for a vehicle. See [Removal](#removal-remove). |
| `void Dispose()` | Stops the background flush loop. |

## Interval gate (`ShouldThrottle`)

Steps, in order:

1. `vehicleId <= 0` throws `ArgumentOutOfRangeException`.
2. If `ThrottleOptions.Enabled` is false, the method returns `false` (accepted) and records no timestamp.
3. Otherwise it reads the minimum interval for `assetType` and the current UTC time.
4. If a last accepted timestamp exists for the vehicle and `now - last < interval`, the method returns `true` (throttled). The stored timestamp is not changed.
5. Otherwise it stores `now` as the new last-accepted time and returns `false` (accepted).

Behavior that follows from these steps:

- The window is anchored on the last **accepted** update. Suppressed updates do not extend it, so the first update after the interval has elapsed is accepted.
- The first update for a vehicle that has no stored timestamp is always accepted.
- Each call writes a `Debug` log entry.
- The read (`TryGetValue`) and the write (indexer set) are separate operations and are not atomic. Two concurrent calls for the same vehicle can both be accepted.

## Buffering (`AddToBuffer`)

Steps, in order:

1. A null `locationDto` throws `ArgumentNullException`.
2. If throttling is disabled, the method returns `false` and buffers nothing.
3. `ShouldThrottle(locationDto.VehicleId, assetType)` is called. If it returns `true`, the update is discarded and the method returns `false`. Throttled updates are not queued.
4. The vehicle's `Channel<LocationDto>` is fetched or created. Each channel is bounded to 100 items (`BufferCapacityPerVehicle`), uses `BoundedChannelFullMode.DropOldest`, and is marked `SingleReader` and `SingleWriter`.
5. `TryWrite` is called. On success the method returns `true`.
6. Any exception is logged as an error and the method returns `false`.

Notes:

- Because the interval gate runs first, only accepted updates reach the buffer. When the minimum interval is longer than the flush interval, a vehicle's buffer normally holds a single item. The capacity of 100 only matters when more than 100 updates are accepted within one flush window.
- With `DropOldest`, `TryWrite` succeeds even when the buffer is full. The oldest item is dropped silently, with no log entry. The "channel full" warning branch is therefore not reached in practice.
- The `false` return value does not distinguish "throttled" from "throttling disabled". A caller cannot use it to decide whether to send directly.

## Background flush loop

The constructor starts `FlushPendingUpdatesLoopAsync` as a fire-and-forget task. The loop does the following on each pass:

1. Waits `CoalesceFlushIntervalMilliseconds` (exposed as `_flushInterval`). The XML comments describe this as 250–500 ms, but the file does not enforce that range.
2. If there are no pending channels, it starts the next pass.
3. Otherwise it calls `FlushChannelAsync` for every pending vehicle and waits for all of them (`Task.WhenAll`).

Cancellation from `Dispose` is caught and logged at `Debug`. Any other exception is logged as an error. Because the `try` wraps the whole `while` loop, an unexpected exception stops the loop for good, and no further flushes happen.

## Coalescing (`FlushChannelAsync`)

Called by the loop (`forceFlush: false`) and by `FlushImmediatelyAsync` (`forceFlush: true`):

1. If the channel's `Reader.Count` is 0, it returns `0`.
2. It reads items with `ReadAllAsync()` into a list and counts them.
3. It sends only the **last** item in the list. The item is sent to group `vehicle-{vehicleId}` as `LocationUpdated`, then again to the same group as `VehicleLocationUpdated`. All earlier items in the batch are discarded without being sent.
4. Any exception is logged as an error and not rethrown.
5. In `finally`, the vehicle's channel is removed from `_pendingUpdates`. A new channel is created on the next accepted update.
6. It returns the number of items read.

The `forceFlush` parameter is accepted but never read. Both callers follow the same code path.

## Removal (`Remove`)

Validates `vehicleId` (`ArgumentOutOfRangeException` if `<= 0`), then removes the vehicle's entries from `_lastUpdateTimes`, `_pendingUpdates`, and `_flushTasks`. Any buffered updates for the vehicle are discarded without being sent.

The XML comment says this also cancels pending flush operations. The code does not cancel anything. `_flushTasks` is never populated, so its removal has no effect.

## Disposal (`Dispose`)

Cancels and disposes the internal `CancellationTokenSource`, which stops the flush loop. It does not flush or clear buffered updates, and it does not wait for the loop to finish.

## Known issues found while reading

These are defects in the current source. They are recorded here and have not been changed.

1. **Flush never completes after the first non-empty read.** `ReadAllAsync()` ends only when the channel writer is completed. Nothing in the file calls `Writer.Complete()` or `TryComplete()`. The `await foreach` in `FlushChannelAsync` therefore waits for more items indefinitely. As a result:
   - The broadcast `SendAsync` calls, which come after the loop, are never reached.
   - The loop's `Task.WhenAll` never finishes, so later flush passes do not run.
   - `FlushImmediatelyAsync` for that vehicle does not return.

   A standalone check on .NET 10 confirmed that `ReadAllAsync` on a channel with two items and no completed writer does not finish after draining them.

2. **Possible lost writes during flush.** The `finally` block removes the channel from `_pendingUpdates` after reading. An `AddToBuffer` call that writes to that channel between the last read and the removal loses its item. This matters once issue 1 is fixed.

3. **Channel usage flags are not honored.** `SingleReader` and `SingleWriter` are set to `true`. However, the flush loop and `FlushImmediatelyAsync` can both read the same channel, and concurrent hub calls can write to it. The channel is not designed for that usage.

4. **`Remove` documentation does not match behavior.** It claims to cancel pending flushes, but it does not (see [Removal](#removal-remove)).

5. **Unused code.** `forceFlush` is never read, `_flushTasks` is never populated, and the "channel full" warning cannot be reached under `DropOldest`.
