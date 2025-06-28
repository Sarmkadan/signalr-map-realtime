// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

/// <summary>
/// Extension methods for DateTime operations and calculations.
/// Provides utilities for common date/time operations needed in tracking applications.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets a friendly human-readable time span (e.g., "5 minutes ago", "2 hours ago").
    /// </summary>
    public static string ToFriendlyTimeSpan(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} seconds ago";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} days ago";

        return dateTime.ToString("MMM d, yyyy");
    }

    /// <summary>
    /// Calculates the duration between two DateTime values.
    /// Useful for measuring elapsed time for tracking sessions.
    /// </summary>
    public static TimeSpan Duration(this DateTime startTime, DateTime endTime)
    {
        return endTime - startTime;
    }

    /// <summary>
    /// Gets the start of the day (midnight).
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday).
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return dateTime.AddDays(-diff).StartOfDay();
    }

    /// <summary>
    /// Gets the end of the week (Sunday).
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the month.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the year.
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year.
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Checks if a DateTime is in the past (before current UTC time).
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime() < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is in the future (after current UTC time).
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime() > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is today.
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Rounds DateTime to the nearest minute.
    /// Useful for rounding location update timestamps.
    /// </summary>
    public static DateTime RoundToNearestMinute(this DateTime dateTime, int minuteInterval = 1)
    {
        var ticks = dateTime.Ticks + (TimeSpan.TicksPerMinute * minuteInterval / 2);
        return new DateTime(ticks - (ticks % (TimeSpan.TicksPerMinute * minuteInterval)));
    }

    /// <summary>
    /// Rounds DateTime to the nearest second.
    /// </summary>
    public static DateTime RoundToNearestSecond(this DateTime dateTime)
    {
        return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond));
    }

    /// <summary>
    /// Checks if a date is between two dates (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Gets the age in years as of a given date.
    /// Useful for calculating age-based metrics.
    /// </summary>
    public static int GetAgeInYears(this DateTime birthDate, DateTime? asOfDate = null)
    {
        asOfDate ??= DateTime.UtcNow;
        var age = asOfDate.Value.Year - birthDate.Year;
        if (birthDate > asOfDate.Value.AddYears(-age)) age--;
        return age;
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)dateTime.ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    public static DateTime FromUnixTimestamp(this long unixTimestamp)
    {
        return new DateTime(1970, 1, 1)
            .AddSeconds(unixTimestamp)
            .ToUniversalTime();
    }

    /// <summary>
    /// Formats DateTime as ISO 8601 string.
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O");
    }
}
