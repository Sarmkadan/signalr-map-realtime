# DateTimeExtensions

A static utility class providing extension methods for `DateTime` and `DateTimeOffset` values. It simplifies common date-time operations such as calculating durations, extracting boundary points (start/end of day, week, month, year), formatting friendly time spans, performing comparisons, rounding, and converting to and from Unix timestamps and ISO 8601 strings.

## API

### ToFriendlyTimeSpan
```csharp
public static string ToFriendlyTimeSpan(this DateTime dateTime, DateTime referenceDate)
```
Returns a human-readable string representing the approximate time span between `dateTime` and `referenceDate` (e.g., "3 days ago", "in 2 hours"). The result is relative to `referenceDate`; a past date yields a past-tense phrase, a future date yields a future-tense phrase.

### Duration
```csharp
public static TimeSpan Duration(this DateTime startDate, DateTime endDate)
```
Returns the absolute `TimeSpan` between `startDate` and `endDate`. The result is always non-negative regardless of argument order.

### StartOfDay
```csharp
public static DateTime StartOfDay(this DateTime dateTime)
```
Returns a `DateTime` representing the start of the same day (00:00:00.000) for the given value. The `DateTimeKind` of the original value is preserved.

### EndOfDay
```csharp
public static DateTime EndOfDay(this DateTime dateTime)
```
Returns a `DateTime` representing the end of the same day (23:59:59.999) for the given value. The `DateTimeKind` of the original value is preserved.

### StartOfWeek
```csharp
public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
```
Returns a `DateTime` at 00:00:00.000 on the first day of the week containing `dateTime`. The week start day defaults to Monday and can be overridden.

### EndOfWeek
```csharp
public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
```
Returns a `DateTime` at 23:59:59.999 on the last day of the week containing `dateTime`, based on the specified `startOfWeek`.

### StartOfMonth
```csharp
public static DateTime StartOfMonth(this DateTime dateTime)
```
Returns a `DateTime` at 00:00:00.000 on the first day of the month for the given value.

### EndOfMonth
```csharp
public static DateTime EndOfMonth(this DateTime dateTime)
```
Returns a `DateTime` at 23:59:59.999 on the last day of the month for the given value.

### StartOfYear
```csharp
public static DateTime StartOfYear(this DateTime dateTime)
```
Returns a `DateTime` at 00:00:00.000 on January 1st of the year for the given value.

### EndOfYear
```csharp
public static DateTime EndOfYear(this DateTime dateTime)
```
Returns a `DateTime` at 23:59:59.999 on December 31st of the year for the given value.

### IsPast
```csharp
public static bool IsPast(this DateTime dateTime)
```
Returns `true` if `dateTime` is earlier than `DateTime.Now`; otherwise `false`.

### IsFuture
```csharp
public static bool IsFuture(this DateTime dateTime)
```
Returns `true` if `dateTime` is later than `DateTime.Now`; otherwise `false`.

### IsToday
```csharp
public static bool IsToday(this DateTime dateTime)
```
Returns `true` if `dateTime` falls on the current date according to `DateTime.Now`; otherwise `false`.

### RoundToNearestMinute
```csharp
public static DateTime RoundToNearestMinute(this DateTime dateTime)
```
Returns a `DateTime` rounded to the nearest whole minute. Values with 30 or more seconds in the minute component round up; otherwise they round down.

### RoundToNearestSecond
```csharp
public static DateTime RoundToNearestSecond(this DateTime dateTime)
```
Returns a `DateTime` rounded to the nearest whole second. Values with 500 or more milliseconds round up; otherwise they round down.

### IsBetween
```csharp
public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate, bool inclusive = true)
```
Returns `true` if `dateTime` lies between `startDate` and `endDate`. When `inclusive` is `true` (default), equality with either boundary returns `true`; when `false`, the comparison is exclusive of both boundaries.

### GetAgeInYears
```csharp
public static int GetAgeInYears(this DateTime birthDate, DateTime referenceDate)
```
Calculates the age in full years from `birthDate` to `referenceDate`. The result is the number of complete years elapsed; if the birthday has not yet occurred in the reference year, the value is one less.

### ToUnixTimestamp
```csharp
public static long ToUnixTimestamp(this DateTime dateTime)
```
Converts the `DateTime` to a Unix timestamp (seconds elapsed since 1970-01-01T00:00:00Z). The input is treated as UTC; non-UTC kinds are converted accordingly.

### FromUnixTimestamp
```csharp
public static DateTime FromUnixTimestamp(this long unixTimestamp)
```
Converts a Unix timestamp (seconds since 1970-01-01T00:00:00Z) to a UTC `DateTime`.

### ToIso8601String
```csharp
public static string ToIso8601String(this DateTime dateTime)
```
Returns the ISO 8601 string representation of the `DateTime` value (e.g., `2025-03-15T14:30:00Z` for UTC or with an appropriate offset for local/unspecified kinds).

## Usage

### Example 1: Displaying relative time for map events
```csharp
DateTime eventTime = new DateTime(2025, 3, 15, 9, 30, 0, DateTimeKind.Utc);
DateTime now = DateTime.UtcNow;

string relativeTime = eventTime.ToFriendlyTimeSpan(now);
bool isRecent = eventTime.IsBetween(now.AddHours(-1), now);

Console.WriteLine($"Event occurred {relativeTime}.");
if (isRecent)
{
    Console.WriteLine("Highlighting on map as recent activity.");
}
```

### Example 2: Aggregating daily statistics within a week boundary
```csharp
DateTime timestamp = DateTime.UtcNow;
DateTime weekStart = timestamp.StartOfWeek(DayOfWeek.Monday);
DateTime weekEnd = timestamp.EndOfWeek(DayOfWeek.Monday);

long unixStart = weekStart.ToUnixTimestamp();
long unixEnd = weekEnd.ToUnixTimestamp();

// Query data store for records between unixStart and unixEnd
Console.WriteLine($"Fetching data from {weekStart.ToIso8601String()} to {weekEnd.ToIso8601String()}");
```

## Notes

- **DateTimeKind awareness**: Methods such as `StartOfDay`, `EndOfDay`, and the week/month/year boundary methods preserve the `DateTimeKind` of the input. When mixing kinds (e.g., comparing a UTC value with `DateTime.Now`), be aware that `IsPast`, `IsFuture`, and `IsToday` use `DateTime.Now` (local time). For consistent UTC-based comparisons, convert inputs to UTC before calling these methods.
- **Rounding edge cases**: `RoundToNearestMinute` and `RoundToNearestSecond` use midpoint rounding away from zero for the fractional component. Values exactly at the midpoint (30 seconds or 500 milliseconds) round up.
- **Unix timestamp conversions**: `ToUnixTimestamp` treats the input as UTC. Passing a `Local` or `Unspecified` kind will convert to UTC before computing the timestamp, which may shift the value by the local offset. `FromUnixTimestamp` always returns a UTC `DateTime`.
- **Age calculation**: `GetAgeInYears` does not account for leap seconds or time-of-day components; it operates strictly on the date portions of `birthDate` and `referenceDate`.
- **Thread safety**: All methods are static and operate on immutable `DateTime` and `TimeSpan` structures. They do not access shared mutable state and are safe to call concurrently from multiple threads.
