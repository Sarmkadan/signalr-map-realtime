#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Extension methods for <see cref="BulkLocationImporter"/> that provide additional functionality
/// for working with location data imports.
/// </summary>
public static class BulkLocationImporterExtensions
{
    /// <summary>
    /// Validates a CSV file for import without actually importing it.
    /// Checks file existence, format, and basic data integrity.
    /// </summary>
    /// <param name="importer">The importer instance</param>
    /// <param name="csvFilePath">Path to the CSV file to validate</param>
    /// <returns>True if the file is valid for import; otherwise false</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="csvFilePath"/> is null or empty</exception>
    public static bool ValidateCsvForImport(this BulkLocationImporter importer, string csvFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(csvFilePath);

        if (!File.Exists(csvFilePath))
        {
            Console.Error.WriteLine($"File not found: {csvFilePath}");
            return false;
        }

        try
        {
            var locations = new List<object>();
            using (var reader = new StreamReader(csvFilePath))
            {
                var header = reader.ReadLine(); // Skip header
                if (header is null)
                {
                    Console.Error.WriteLine("CSV file is empty or has no header");
                    return false;
                }

                string? line;
                int lineNumber = 2;
                int validCount = 0;
                int invalidCount = 0;

                while ((line = reader.ReadLine()) is not null)
                {
                    try
                    {
                        var parts = line.Split(',');
                        if (parts.Length < 7)
                        {
                            Console.Error.WriteLine($"Invalid format at line {lineNumber}: expected 7+ columns");
                            invalidCount++;
                            continue;
                        }

                        if (!Guid.TryParse(parts[0], out _))
                        {
                            Console.Error.WriteLine($"Invalid vehicle ID at line {lineNumber}");
                            invalidCount++;
                            continue;
                        }

                        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            Console.Error.WriteLine($"Invalid latitude at line {lineNumber}");
                            invalidCount++;
                            continue;
                        }

                        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            Console.Error.WriteLine($"Invalid longitude at line {lineNumber}");
                            invalidCount++;
                            continue;
                        }

                        validCount++;
                                lineNumber++;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error parsing line {lineNumber}: {ex.Message}");
                        invalidCount++;
                    }
                }

                Console.WriteLine($"Validation complete: {validCount} valid, {invalidCount} invalid lines");
                return invalidCount == 0;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets statistics about a CSV file without importing it.
    /// </summary>
    /// <param name="importer">The importer instance</param>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <returns>Location statistics including count, date range, and bounding box</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="csvFilePath"/> is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown if <paramref name="csvFilePath"/> does not exist</exception>
    public static LocationImportStatistics GetCsvStatistics(this BulkLocationImporter importer, string csvFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(csvFilePath);

        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException("CSV file not found", csvFilePath);
        }

        var stats = new LocationImportStatistics
        {
            FilePath = csvFilePath,
            IsValid = true
        };

        try
        {
            using (var reader = new StreamReader(csvFilePath))
            {
                var header = reader.ReadLine(); // Skip header
                if (header is null)
                {
                    stats.IsValid = false;
                    stats.ErrorMessage = "CSV file is empty";
                    return stats;
                }

                string? line;
                int lineNumber = 2;

                while ((line = reader.ReadLine()) is not null)
                {
                    try
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 7)
                        {
                            if (Guid.TryParse(parts[0], out var vehicleId))
                            {
                                stats.VehicleIds.Add(vehicleId);
                            }

                            if (double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
                            {
                                stats.MinLatitude = Math.Min(stats.MinLatitude, latitude);
                                stats.MaxLatitude = Math.Max(stats.MaxLatitude, latitude);
                            }

                            if (double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                            {
                                stats.MinLongitude = Math.Min(stats.MinLongitude, longitude);
                                stats.MaxLongitude = Math.Max(stats.MaxLongitude, longitude);
                            }

                            if (double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var speed))
                            {
                                stats.TotalSpeed += speed;
                                stats.MaxSpeed = Math.Max(stats.MaxSpeed, speed);
                            }

                            if (DateTime.TryParse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
                            {
                                stats.EarliestTimestamp = stats.EarliestTimestamp == default
                                    ? timestamp
                                    : (timestamp < stats.EarliestTimestamp ? timestamp : stats.EarliestTimestamp);
                                stats.LatestTimestamp = stats.LatestTimestamp == default
                                    ? timestamp
                                    : (timestamp > stats.LatestTimestamp ? timestamp : stats.LatestTimestamp);
                            }

                            stats.TotalLocations++;
                        }
                    }
                    catch (Exception ex)
                    {
                        stats.InvalidLines++;
                        stats.ErrorMessage = ex.Message;
                    }

                            lineNumber++;
                }

                if (stats.TotalLocations > 0)
                {
                    stats.AverageSpeed = stats.TotalSpeed / stats.TotalLocations;
                    stats.VehicleCount = stats.VehicleIds.Distinct().Count();
                }
            }
        }
        catch (Exception ex)
        {
            stats.IsValid = false;
            stats.ErrorMessage = ex.Message;
        }

        return stats;
    }

    /// <summary>
    /// Imports locations from multiple CSV files in sequence.
    /// </summary>
    /// <param name="importer">The importer instance</param>
    /// <param name="csvFilePaths">Collection of CSV file paths to import</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="csvFilePaths"/> is null</exception>
    /// <exception cref="InvalidOperationException">Thrown if any import fails</exception>
    public static async Task ImportFromMultipleCsvFiles(this BulkLocationImporter importer, IEnumerable<string> csvFilePaths)
    {
        ArgumentNullException.ThrowIfNull(csvFilePaths);

        var files = csvFilePaths.ToList();
        if (files.Count == 0)
        {
            Console.WriteLine("No CSV files provided for import");
            return;
        }

        Console.WriteLine($"=== Importing from {files.Count} CSV file(s) ===\n");

        int filesProcessed = 0;
        int totalSuccess = 0;
        int totalFailures = 0;

        foreach (var filePath in files)
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File not found: {filePath}");
                continue;
            }

            Console.WriteLine($"Importing: {Path.GetFileName(filePath)} ({filePath})");

            try
            {
                await importer.ImportFromCsv(filePath).ConfigureAwait(false);
                totalSuccess++;
                filesProcessed++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to import {filePath}: {ex.Message}");
                totalFailures++;
            }
        }

        Console.WriteLine($"\n=== Import Summary ===");
        Console.WriteLine($"Files processed: {filesProcessed}");
        Console.WriteLine($"Successful imports: {totalSuccess}");
        Console.WriteLine($"Failed imports: {totalFailures}");

        if (totalFailures > 0)
        {
            throw new InvalidOperationException($"Failed to import {totalFailures} file(s)");
        }
    }

    /// <summary>
    /// Filters locations by date range before importing.
    /// </summary>
    /// <param name="importer">The importer instance</param>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <param name="startDate">Start date for filtering (inclusive)</param>
    /// <param name="endDate">End date for filtering (inclusive)</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="csvFilePath"/> is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startDate"/> is after <paramref name="endDate"/></exception>
    public static async Task ImportFromCsvWithDateFilter(
        this BulkLocationImporter importer,
        string csvFilePath,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrEmpty(csvFilePath);

        if (startDate > endDate)
        {
            throw new ArgumentOutOfRangeException(nameof(startDate), "Start date must be before or equal to end date");
        }

        if (!File.Exists(csvFilePath))
        {
            Console.Error.WriteLine($"File not found: {csvFilePath}");
            return;
        }

        Console.WriteLine($"=== Bulk Location Importer with Date Filter ===\n");
        Console.WriteLine($"Importing from: {csvFilePath}");
        Console.WriteLine($"Date range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}\n");

        try
        {
            var locations = new List<object>();
            using (var reader = new StreamReader(csvFilePath))
            {
                var header = reader.ReadLine(); // Skip header
                string? line;
                int lineNumber = 2;

                while ((line = reader.ReadLine()) is not null)
                {
                    try
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 7 && DateTime.TryParse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
                        {
                            if (timestamp >= startDate && timestamp <= endDate)
                            {
                                if (Guid.TryParse(parts[0], out var vehicleId) &&
                                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
                                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                                {
                                    var location = new LocationData
                                    {
                                        VehicleId = vehicleId,
                                        Latitude = latitude,
                                        Longitude = longitude,
                                        Accuracy = double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var acc) ? acc : 10.0,
                                        Speed = double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var spd) ? spd : 0,
                                        Heading = int.TryParse(parts[4], out var hdg) ? hdg : 0,
                                        Type = "GPS",
                                        Timestamp = timestamp
                                    };
                                    locations.Add(location);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Error parsing line {lineNumber}: {ex.Message}");
                    }

                            lineNumber++;
                }
            }

            Console.WriteLine($"Loaded {locations.Count} locations within date range");
            if (locations.Count > 0)
        {
            await importer.ImportLocations(locations).ConfigureAwait(false);
        }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }
}

/// <summary>
/// Statistics about a location import operation.
/// </summary>
public sealed class LocationImportStatistics
{
    /// <summary>Gets or sets the file path that was analyzed</summary>
    public string? FilePath { get; set; }

    /// <summary>Gets or sets whether the file is valid for import</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets or sets any error message from validation</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the total number of locations found</summary>
    public int TotalLocations { get; set; }

    /// <summary>Gets the number of invalid lines encountered</summary>
    public int InvalidLines { get; set; }

    /// <summary>Gets the number of unique vehicle IDs found</summary>
    public int VehicleCount { get; set; }

    /// <summary>Gets the collection of unique vehicle IDs</summary>
    public ICollection<Guid> VehicleIds { get; } = new List<Guid>();

    /// <summary>Gets the minimum latitude value</summary>
    public double MinLatitude { get; set; } = double.MaxValue;

    /// <summary>Gets the maximum latitude value</summary>
    public double MaxLatitude { get; set; } = double.MinValue;

    /// <summary>Gets the minimum longitude value</summary>
    public double MinLongitude { get; set; } = double.MaxValue;

    /// <summary>Gets the maximum longitude value</summary>
    public double MaxLongitude { get; set; } = double.MinValue;

    /// <summary>Gets the earliest timestamp found</summary>
    public DateTime EarliestTimestamp { get; set; }

    /// <summary>Gets the latest timestamp found</summary>
    public DateTime LatestTimestamp { get; set; }

    /// <summary>Gets the average speed across all locations</summary>
    public double AverageSpeed { get; set; }

    /// <summary>Gets the maximum speed encountered</summary>
    public double MaxSpeed { get; set; }

    /// <summary>Gets the total speed sum (used for calculating average)</summary>
    public double TotalSpeed { get; set; }
}