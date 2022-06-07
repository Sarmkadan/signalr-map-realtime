# GeoLocationExtensionsTests

Unit tests for extension methods in `GeoLocationExtensions` that provide geospatial calculations and conversions. These tests verify the behavior of distance calculations between geographic coordinates, unit conversions between kilometers and miles, and radius-based proximity checks.

## API

### `DistanceBetween_SameCoordinates_ReturnsZero`
Verifies that the distance between identical geographic coordinates is zero. This test ensures that the distance calculation logic correctly handles the trivial case where both points are the same.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: Does not throw under normal test conditions

### `DistanceBetween_LondonToNewYork_ReturnsApproximateDistance`
Validates that the distance calculation between London and New York returns a value close to the known approximate distance. This test confirms the accuracy of the Haversine formula implementation used for great-circle distance calculations.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: Does not throw under normal test conditions

### `KilometersToMiles_OneKilometer_ReturnsExpectedMiles`
Ensures that converting one kilometer to miles produces the expected result (approximately 0.621371 miles). This test checks the correctness of the unit conversion logic.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: Does not throw under normal test conditions

### `IsWithinRadius_PointBeyondRadius_ReturnsFalse`
Confirms that a point located beyond a specified radius from a reference point is correctly identified as outside the radius. This test validates the proximity-checking logic.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: Does not throw under normal test conditions

## Usage
