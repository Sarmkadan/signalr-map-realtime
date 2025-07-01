// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Bulk imports historical location data from CSV files.
/// Useful for importing legacy GPS tracking data or testing with realistic datasets.
/// </summary>
public class BulkLocationImporter
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private const int BatchSize = 100;

    public BulkLocationImporter(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task ImportFromCsv(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            Console.Error.WriteLine($"File not found: {csvFilePath}");
            return;
        }

        Console.WriteLine($"=== Bulk Location Importer ===\n");
        Console.WriteLine($"Importing from: {csvFilePath}\n");

        try
        {
            var locations = LoadLocationsFromCsv(csvFilePath);
            Console.WriteLine($"Loaded {locations.Count} locations");

            await ImportLocationsInBatches(locations);

            Console.WriteLine("\n✓ Import completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads locations from a CSV file.
    /// Expected CSV format: VehicleId,Latitude,Longitude,Speed,Heading,Accuracy,Timestamp
    /// </summary>
    private List<object> LoadLocationsFromCsv(string filePath)
    {
        var locations = new List<object>();

        using (var reader = new StreamReader(filePath))
        {
            var header = reader.ReadLine(); // Skip header

            string? line;
            int lineNumber = 2;

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var parts = line.Split(',');

                    if (parts.Length < 7)
                    {
                        Console.WriteLine($"Warning: Invalid format at line {lineNumber}");
                        continue;
                    }

                    if (!Guid.TryParse(parts[0], out var vehicleId))
                    {
                        Console.WriteLine($"Warning: Invalid vehicle ID at line {lineNumber}");
                        continue;
                    }

                    if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
                    {
                        Console.WriteLine($"Warning: Invalid latitude at line {lineNumber}");
                        continue;
                    }

                    if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                    {
                        Console.WriteLine($"Warning: Invalid longitude at line {lineNumber}");
                        continue;
                    }

                    if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var speed))
                    {
                        speed = 0;
                    }

                    if (!int.TryParse(parts[4], out var heading))
                    {
                        heading = 0;
                    }

                    if (!double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var accuracy))
                    {
                        accuracy = 10.0;
                    }

                    if (!DateTime.TryParse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
                    {
                        timestamp = DateTime.UtcNow;
                    }

                    var location = new
                    {
                        vehicleId = vehicleId.ToString(),
                        latitude = latitude,
                        longitude = longitude,
                        accuracy = accuracy,
                        speed = speed,
                        heading = heading,
                        type = "GPS",
                        timestamp = timestamp
                    };

                    locations.Add(location);
                    lineNumber++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error parsing line {lineNumber}: {ex.Message}");
                }
            }
        }

        return locations;
    }

    /// <summary>
    /// Imports locations in batches to optimize API calls.
    /// </summary>
    private async Task ImportLocationsInBatches(List<object> locations)
    {
        var batches = locations
            .Select((location, index) => new { location, index })
            .GroupBy(x => x.index / BatchSize)
            .Select(g => g.Select(x => x.location).ToList())
            .ToList();

        Console.WriteLine($"Importing {locations.Count} locations in {batches.Count} batches...\n");

        int successCount = 0;
        int failureCount = 0;

        for (int i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            Console.WriteLine($"[{i + 1}/{batches.Count}] Processing batch of {batch.Count} locations...");

            foreach (var location in batch)
            {
                var success = await SendLocation(location);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }

            if (i < batches.Count - 1)
            {
                await Task.Delay(500); // Rate limiting between batches
            }
        }

        Console.WriteLine($"\nImport Results:");
        Console.WriteLine($"  ✓ Successful: {successCount}");
        Console.WriteLine($"  ✗ Failed: {failureCount}");
    }

    /// <summary>
    /// Sends a single location to the API.
    /// </summary>
    private async Task<bool> SendLocation(object location)
    {
        try
        {
            var json = JsonSerializer.Serialize(location);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", _apiKey);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/locations", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a sample CSV file for testing.
    /// </summary>
    public static void GenerateSampleCsv(string outputPath, int locationCount = 1000)
    {
        var random = new Random();
        var vehicleIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        using (var writer = new StreamWriter(outputPath))
        {
            writer.WriteLine("VehicleId,Latitude,Longitude,Speed,Heading,Accuracy,Timestamp");

            var baseTime = DateTime.UtcNow.AddHours(-24);

            for (int i = 0; i < locationCount; i++)
            {
                var vehicleId = vehicleIds[random.Next(vehicleIds.Count)];
                var latitude = 40.7128 + (random.NextDouble() - 0.5) * 0.1;
                var longitude = -74.0060 + (random.NextDouble() - 0.5) * 0.1;
                var speed = random.Next(0, 100);
                var heading = random.Next(360);
                var accuracy = 5 + random.NextDouble() * 10;
                var timestamp = baseTime.AddSeconds(i * 30);

                writer.WriteLine(
                    $"{vehicleId},{latitude:F8},{longitude:F8},{speed},{heading},{accuracy:F2},{timestamp:O}"
                );
            }
        }

        Console.WriteLine($"✓ Sample CSV generated: {outputPath}");
    }

    /// <summary>
    /// Main entry point for the importer.
    /// Usage: BulkLocationImporter.exe https://localhost:5001 api-key data.csv
    /// </summary>
    public static async Task Main(string[] args)
    {
        if (args.Length < 3 && args.FirstOrDefault() != "generate")
        {
            Console.WriteLine("Usage: BulkLocationImporter.exe <base-url> <api-key> <csv-file>");
            Console.WriteLine("   or: BulkLocationImporter.exe generate <output-file> [count]");
            return;
        }

        if (args[0] == "generate")
        {
            var outputFile = args[1];
            var count = args.Length > 2 ? int.Parse(args[2]) : 1000;
            GenerateSampleCsv(outputFile, count);
            return;
        }

        var baseUrl = args[0];
        var apiKey = args[1];
        var csvFile = args[2];

        var importer = new BulkLocationImporter(baseUrl, apiKey);
        await importer.ImportFromCsv(csvFile);
    }
}
