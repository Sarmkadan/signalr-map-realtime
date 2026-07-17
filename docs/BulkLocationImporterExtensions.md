# BulkLocationImporterExtensions

Static utility class providing CSV-based bulk import functionality for location data in SignalR real-time mapping applications. Designed to validate, analyze, and import large datasets of vehicle location records with optional date filtering.

## API

### `ValidateCsvForImport`

Validates whether the specified CSV file contains location data in the expected format without performing any imports.

**Parameters**
- `filePath` (string): Absolute or relative path to the CSV file to validate.

**Return Value**
- `bool`: `true` if the file is valid and contains location data; `false` otherwise.

**Exceptions**
- Throws `ArgumentNullException` if `filePath` is `null`.
- Throws `FileNotFoundException` if the file does not exist.
- Throws `UnauthorizedAccessException` if the caller lacks permissions to read the file.
- Throws `InvalidDataException` if the file is not a valid CSV or contains malformed location records.

---

### `GetCsvStatistics`

Analyzes the specified CSV file and returns summary statistics about the location data it contains.

**Parameters**
- `filePath` (string): Absolute or relative path to the CSV file to analyze.

**Return Value**
- `LocationImportStatistics`: An instance containing parsed statistics such as location count, vehicle IDs, coordinate ranges, and timestamp bounds.

**Exceptions**
- Throws `ArgumentNullException` if `filePath` is `null`.
- Throws `FileNotFoundException` if the file does not exist.
- Throws `UnauthorizedAccessException` if the caller lacks permissions to read the file.
- Throws `InvalidDataException` if the file is not a valid CSV or contains malformed records.

---

### `ImportFromMultipleCsvFiles`

Imports location data from multiple CSV files into the system, aggregating results across all files.

**Parameters**
- `filePaths` (IEnumerable<string>): Collection of absolute or relative paths to CSV files to import.

**Return Value**
- `Task`: A task that completes when all files have been processed.

**Exceptions**
- Throws `ArgumentNullException` if `filePaths` is `null`.
- Throws `AggregateException` containing one or more file-level exceptions if any file fails to import.
- Throws `InvalidOperationException` if the import operation cannot proceed due to invalid internal state.

---

### `ImportFromCsvWithDateFilter`

Imports location data from a CSV file, filtering records to only those within a specified date range.

**Parameters**
- `filePath` (string): Absolute or relative path to the CSV file to import.
- `startDate` (DateTime): Inclusive lower bound for record timestamps.
- `endDate` (DateTime): Inclusive upper bound for record timestamps.

**Return Value**
- `Task`: A task that completes when the file has been processed and filtered records imported.

**Exceptions**
- Throws `ArgumentNullException` if `filePath` is `null`.
- Throws `FileNotFoundException` if the file does not exist.
- Throws `UnauthorizedAccessException` if the caller lacks permissions to read the file.
- Throws `ArgumentOutOfRangeException` if `startDate` is after `endDate`.
- Throws `InvalidDataException` if the file is not a valid CSV or contains malformed records.

---

### `FilePath`

Gets the file path associated with the current import operation or analysis.

**Value**
- `string?`: The file path used in the most recent call to `GetCsvStatistics`, `ValidateCsvForImport`, or an import method. `null` if no operation has been performed.

---

### `IsValid`

Indicates whether the last validation or import operation succeeded without errors.

**Value**
- `bool`: `true` if the last operation completed successfully; otherwise, `false`.

---

### `ErrorMessage`

Gets the error message from the last failed operation, if any.

**Value**
- `string?`: A human-readable description of the error encountered during the last operation. `null` if no error occurred.

---

### `TotalLocations`

Gets the total number of valid location records processed in the last import or analysis.

**Value**
- `int`: The count of valid location records. Zero if no operation has been performed or if all records were invalid.

---

### `InvalidLines`

Gets the number of lines in the CSV that were skipped due to parsing or validation errors.

**Value**
- `int`: The count of invalid lines encountered during the last operation.

---

### `VehicleCount`

Gets the number of unique vehicles identified in the processed data.

**Value**
- `int`: The count of distinct vehicle identifiers. Zero if no valid records were processed.

---

### `VehicleIds`

Gets the collection of unique vehicle identifiers found in the processed data.

**Value**
- `ICollection<Guid>`: A read-only collection of vehicle GUIDs. Empty if no valid records were processed.

---

### `MinLatitude`

Gets the minimum latitude value among all valid location records.

**Value**
- `double`: The smallest latitude coordinate. `double.NaN` if no valid records were processed.

---

### `MaxLatitude`

Gets the maximum latitude value among all valid location records.

**Value**
- `double`: The largest latitude coordinate. `double.NaN` if no valid records were processed.

---
### `MinLongitude`

Gets the minimum longitude value among all valid location records.

**Value**
- `double`: The smallest longitude coordinate. `double.NaN` if no valid records were processed.

---
### `MaxLongitude`

Gets the maximum longitude value among all valid location records.

**Value**
- `double`: The largest longitude coordinate. `double.NaN` if no valid records were processed.

---
### `EarliestTimestamp`

Gets the earliest timestamp among all valid location records.

**Value**
- `DateTime`: The oldest timestamp. `DateTime.MinValue` if no valid records were processed.

---
### `LatestTimestamp`

Gets the latest timestamp among all valid location records.

**Value**
- `DateTime`: The most recent timestamp. `DateTime.MinValue` if no valid records were processed.

---
### `AverageSpeed`

Gets the average speed across all valid location records.

**Value**
- `double`: The mean speed in consistent units. `0.0` if no valid records were processed.

---
### `MaxSpeed`

Gets the maximum speed observed among all valid location records.

**Value**
- `double`: The highest speed value. `0.0` if no valid records were processed.

---
### `TotalSpeed`

Gets the sum of all speed values across valid location records.

**Value**
- `double`: The cumulative speed. `0.0` if no valid records were processed.

## Usage

### Example 1: Validate and Import a Single CSV File
