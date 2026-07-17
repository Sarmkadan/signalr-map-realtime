# AssetControllerTestsValidation

Provides centralized validation logic for asset controller test scenarios, offering both boolean checks and exception-throwing assertions. The type exposes overloaded static members that accept different parameter sets to validate asset-related data, ensuring consistency across integration tests without duplicating validation rules.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(Asset asset)
public static IReadOnlyList<string> Validate(AssetUpdateDto dto)
public static IReadOnlyList<string> Validate(AssetLocation location)
public static IReadOnlyList<string> Validate(AssetMetadata metadata)
```

Returns a read-only list of validation error messages for the given object. An empty list indicates the object is fully valid. Each overload targets a specific asset-related type and applies the appropriate validation rules for that context.

**Parameters**
- `asset` — The `Asset` instance to validate.
- `dto` — The `AssetUpdateRequest` DTO to validate.
- `location` — The `AssetLocation` to validate.
- `metadata` — The `AssetMetadata` to validate.

**Returns**
`IReadOnlyList<string>` — A list of error messages. Empty when valid.

**Throws**
- `ArgumentNullException` — When the argument is `null`.

---

### IsValid

```csharp
public static bool IsValid(Asset asset)
public static bool IsValid(AssetUpdateRequest dto)
public static bool IsValid(AssetLocation location)
```

Returns `true` if the given object passes all validation rules; otherwise `false`. Delegates to the corresponding `Validate` overload and checks whether the returned list is empty.

**Parameters**
- `asset` — The `Asset` instance to check.
- `dto` — The `AssetUpdateRequest` DTO to check.
- `location` — The `AssetLocation` to check.

**Returns**
`bool` — `true` if valid, `false` otherwise.

**Exceptions**
- `ArgumentNullException` — When the argument is `null`.

---

### EnsureValid

```csharp
public static void EnsureValid(Asset asset)
public static void EnsureValid(AssetUpdateRequest dto)
public static void EnsureValid(AssetLocation location)
```

Asserts that the given object is valid. If validation fails, throws an exception containing the aggregated error messages. Intended for use in test arrange/act phases where invalid state should halt execution immediately.

**Parameters**
- `asset` — The `Asset` instance to assert.
- `dto` — The `AssetUpdateRequest` DTO to assert.
- `location` — The `AssetLocation` to assert.

**Exceptions**
- `ArgumentNullException` — When the argument is `null`.
- `ValidationException` — When the object fails one or more validation rules. The exception message aggregates all failure strings.

## Usage

### Example 1: Asserting an asset is valid before acting

```csharp
var asset = new Asset
{
    Id = "asset-001",
    Name = "Excavator",
    Location = new AssetLocation { Latitude = 45.0, Longitude = -93.0 }
};

// Halt the test immediately if the asset is invalid
AssetControllerTestsValidation.EnsureValid(asset);

// Proceed with the controller action
var result = await controller.UpdateAssetLocation(asset.Id, new AssetLocation { Latitude = 46.0, Longitude = -94.0 });
```

### Example 2: Collecting and inspecting validation failures

```csharp
var dto = new AssetUpdateRequest
{
    AssetId = "",
    Metadata = null
};

IReadOnlyList<string> errors = AssetControllerTestsValidation.Validate(dto);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        TestContext.WriteLine($"Validation failure: {error}");
    }
    Assert.Fail("AssetUpdateRequest should be valid before proceeding.");
}
```

## Notes

- All overloads are static and stateless; they are safe to call concurrently from multiple test threads without any synchronization.
- The `Validate` overloads never return `null` — they return an empty list for valid input. Callers should check `Count == 0` rather than comparing to `null`.
- `EnsureValid` throws a `ValidationException` whose message is built from the concatenated error strings returned by `Validate`. Tests that expect specific failure messages should use `Validate` directly and inspect the list.
- The `IsValid` overloads are convenience wrappers around `Validate` and do not cache results; repeated calls on the same instance will re-run all validation rules.
- Input objects are not mutated by any of these methods.
- The `AssetMetadata` type has a `Validate` overload but no corresponding `IsValid` or `EnsureValid` overload. Use `Validate(AssetMetadata)` and check the result list manually when working with metadata in isolation.
