#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

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
    /// <param name="dateTime">The date/time to format.</param>
    /// <returns>A friendly time span string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static string ToFriendlyTimeSpan(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));

        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        return timeSpan.TotalSeconds switch
        {
            < 60 => $"{(int)timeSpan.TotalSeconds} seconds ago",
            < 3600 => $"{(int)timeSpan.TotalMinutes} minutes ago",
            < 86400 => $"{(int)timeSpan.TotalHours} hours ago",
            < 2592000 => $"{(int)timeSpan.TotalDays} days ago",
            _ => dateTime.ToString("MMM d, yyyy")
        };
    }

    /// <summary>
    /// Calculates the duration between two DateTime values.
    /// Useful for measuring elapsed time for tracking sessions.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>The time span between the two dates.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startTime"/> is after <paramref name="endTime"/>.</exception>
    public static TimeSpan Duration(this DateTime startTime, DateTime endTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(endTime, startTime, nameof(endTime));
        return endTime - startTime;
    }

    /// <summary>
    /// Gets the start of the day (midnight).
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing midnight of the same day.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing the end of the day.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday).
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing the start of the week (Monday).</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));

        var diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0)
            diff += 7;

        return dateTime.AddDays(-diff).StartOfDay();
    }

    /// <summary>
    /// Gets the end of the week (Sunday).
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing the end of the week (Sunday).</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.StartOfWeek().AddDays(7).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month.
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing the first day of the month at midnight.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month.
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing the last day of the month at 23:59:59.999.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the year.
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing January 1st of the same year at midnight.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year.
    /// </summary>
    /// <param name="dateTime">The date/time value.</param>
    /// <returns>A DateTime representing December 31st of the same year at 23:59:59.999.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Checks if a DateTime is in the past (before current UTC time).
    /// </summary>
    /// <param name="dateTime">The date/time to check.</param>
    /// <returns>True if the date/time is in the past; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static bool IsPast(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.ToUniversalTime() < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is in the future (after current UTC time).
    /// </summary>
    /// <param name="dateTime">The date/time to check.</param>
    /// <returns>True if the date/time is in the future; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static bool IsFuture(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.ToUniversalTime() > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is today.
    /// </summary>
    /// <param name="dateTime">The date/time to check.</param>
    /// <returns>True if the date/time is today; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static bool IsToday(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Rounds DateTime to the nearest minute.
    /// Useful for rounding location update timestamps.
    /// </summary>
    /// <param name="dateTime">The date/time to round.</param>
    /// <param name="minuteInterval">The rounding interval in minutes. Default is 1.</param>
    /// <returns>A DateTime rounded to the nearest minute interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minuteInterval"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime RoundToNearestMinute(this DateTime dateTime, int minuteInterval = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minuteInterval, 1, nameof(minuteInterval));
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));

        var ticks = dateTime.Ticks + (TimeSpan.TicksPerMinute * minuteInterval / 2);
        return new DateTime(ticks - (ticks % (TimeSpan.TicksPerMinute * minuteInterval)));
    }

    /// <summary>
    /// Rounds DateTime to the nearest second.
    /// </summary>
    /// <param name="dateTime">The date/time to round.</param>
    /// <returns>A DateTime rounded to the nearest second.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static DateTime RoundToNearestSecond(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond));
    }

    /// <summary>
    /// Checks if a date is between two dates (inclusive).
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>True if the date is between the start and end dates; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when any date is invalid.</exception>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfLessThan(startDate, DateTime.MinValue, nameof(startDate));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startDate, DateTime.MaxValue, nameof(startDate));
        ArgumentOutOfRangeException.ThrowIfLessThan(endDate, DateTime.MinValue, nameof(endDate));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endDate, DateTime.MaxValue, nameof(endDate));

        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Gets the age in years as of a given date.
    /// Useful for calculating age-based metrics.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <param name="asOfDate">The date to calculate age as of. Defaults to UTC now.</param>
    /// <returns>The age in years.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="birthDate"/> is invalid or is after <paramref name="asOfDate"/>.</exception>
    public static int GetAgeInYears(this DateTime birthDate, DateTime? asOfDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(birthDate, DateTime.MinValue, nameof(birthDate));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(birthDate, DateTime.MaxValue, nameof(birthDate));

        asOfDate ??= DateTime.UtcNow;
        ArgumentOutOfRangeException.ThrowIfLessThan(asOfDate.Value, birthDate, nameof(asOfDate));

        var age = asOfDate.Value.Year - birthDate.Year;
        if (birthDate > asOfDate.Value.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    /// <param name="dateTime">The date/time to convert.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));

        return (long)dateTime.ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    /// <param name="unixTimestamp">The Unix timestamp in seconds.</param>
    /// <returns>A DateTime representing the timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unixTimestamp"/> is negative.</exception>
    public static DateTime FromUnixTimestamp(this long unixTimestamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(unixTimestamp, nameof(unixTimestamp));
        return new DateTime(1970, 1, 1)
            .AddSeconds(unixTimestamp)
            .ToUniversalTime();
    }

    /// <summary>
    /// Formats DateTime as ISO 8601 string.
    /// </summary>
    /// <param name="dateTime">The date/time to format.</param>
    /// <returns>An ISO 8601 formatted string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dateTime"/> is invalid.</exception>
    public static string ToIso8601String(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.MaxValue, nameof(dateTime));
        return dateTime.ToUniversalTime().ToString("O");
    }
}