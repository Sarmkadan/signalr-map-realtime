# LocationConstants

Static class in `SignalRMapRealtime.Constants` that holds fixed geographic constants used by location tracking and coordinate handling. It contains only `const` fields, so it cannot be instantiated and needs no configuration. The values define the valid coordinate range and the Earth radius used for distance calculations.

## Constants

### `MinLatitude`
Minimum valid latitude in decimal degrees.  
**Type:** `double`  
**Value:** `-90.0`

### `MaxLatitude`
Maximum valid latitude in decimal degrees.  
**Type:** `double`  
**Value:** `90.0`

### `MinLongitude`
Minimum valid longitude in decimal degrees.  
**Type:** `double`  
**Value:** `-180.0`

### `MaxLongitude`
Maximum valid longitude in decimal degrees.  
**Type:** `double`  
**Value:** `180.0`

### `EarthRadiusKm`
Earth's radius in kilometers, used for distance calculations.  
**Type:** `double`  
**Value:** `6371.0`

## Usage notes

- The latitude and longitude bounds together define the valid coordinate range. Range checks compare values against these bounds inclusively.
- `EarthRadiusKm` is a mean-radius approximation. Distance results computed with it are approximate.
- The source file has a comment stating that tunable values were moved to `LocationOptions`, which is bound from configuration. The constants kept here are for backward compatibility, and the comment says they will be removed in a future major version. The file defines no other constants.
