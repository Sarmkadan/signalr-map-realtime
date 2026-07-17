# LocationControllerTestsValidation

Provides centralized validation logic for test data used in `LocationController` integration tests. This type exposes static methods and properties that verify the correctness of location-related payloads, ensuring that test inputs conform to expected formats and constraints before being submitted to the controller. It is designed to be used within test arrange and assert phases to fail fast on malformed data.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(object? model)
public static IReadOnlyList<string> Validate(object? model, string? paramName)
public static IReadOnlyList<string> Validate(object? model, string? paramName, bool throwOnError)
```

Validates a test model object against the rules defined for location controller inputs. Returns a read-only list of validation error messages. When `throwOnError` is `true` and validation fails, an exception is thrown. The `paramName` overload allows associating errors with a specific parameter name in the resulting messages or exception.

**Parameters:**
- `model` — The object to validate. May be `null`, in which case validation will produce a null-reference error.
- `paramName` — Optional name to identify the parameter in error output.
- `throwOnError` — When `true`, throws immediately on the first validation failure instead of collecting all errors.

**Returns:** A read-only list of strings, each representing a distinct validation failure. An empty list indicates the model is valid.

**Throws:** Throws an exception (typically `ArgumentException` or a custom validation exception) when `throwOnError` is `true` and one or more validation errors are present.

### IsValid

```csharp
public static bool IsValid(object? model)
public static bool IsValid(object? model, string? paramName)
public static bool IsValid(object? model, string? paramName, out IReadOnlyList<string> errors)
```

Determines whether a test model object passes all validation rules. The overload with an `out` parameter provides access to the specific errors without throwing.

**Parameters:**
- `model` — The object to validate.
- `paramName` — Optional parameter name for error context.
- `errors` — When this overload is used, receives the list of validation error messages if the model is invalid; `null` or empty if valid.

**Returns:** `true` if the model satisfies all validation rules; otherwise `false`.

### EnsureValid

```csharp
public static void EnsureValid(object? model)
public static void EnsureValid(object? model, string? paramName)
public static void EnsureValid(object? model, string? paramName, out IReadOnlyList<string> errors)
```

Asserts that a test model is valid, throwing an exception if it is not. The `out` overload provides the error details in the exception case while still throwing.

**Parameters:**
- `model` — The object to validate.
- `paramName` — Optional parameter name for error context.
- `errors` — Receives the list of validation errors when the model is invalid.

**Throws:** Throws an exception when validation fails. The exception type and message incorporate the validation errors and, if provided, the `paramName`.

## Usage

### Example 1: Asserting validity in a test arrange phase

```csharp
var locationUpdate = new LocationUpdate
{
    Latitude = 47.6062,
    Longitude = -122.3321,
    Timestamp = DateTimeOffset.UtcNow
};

// Fail fast if the test input is malformed
LocationControllerTestsValidation.EnsureValid(locationUpdate, nameof(locationUpdate));

var controller = new LocationController();
var result = await controller.PostLocationUpdate(locationUpdate);

Assert.IsNotNull(result);
```

### Example 2: Collecting errors for detailed assertion messages

```csharp
var invalidPayload = new LocationUpdate
{
    Latitude = 200.0,  // out of range
    Longitude = null,
    Timestamp = default
};

bool isValid = LocationControllerTestsValidation.IsValid(
    invalidPayload,
    nameof(invalidPayload),
    out IReadOnlyList<string> errors);

Assert.IsFalse(isValid);
Assert.IsTrue(errors.Any(e => e.Contains("Latitude")));
Assert.IsTrue(errors.Any(e => e.Contains("Longitude")));
Assert.IsTrue(errors.Any(e => e.Contains("Timestamp")));
```

## Notes

- All members are static and stateless; they are safe to call concurrently from multiple test threads without synchronization.
- When `model` is `null`, validation treats this as a failure case. The resulting error message will indicate a null argument, and `EnsureValid` will throw.
- The `throwOnError` parameter on `Validate` enables an early-exit strategy: use it when only the first error matters, or when validating large object graphs where collecting all errors would be unnecessarily expensive.
- The `out IReadOnlyList<string> errors` overloads on `IsValid` and `EnsureValid` allow inspection of failure details without catching exceptions, which can simplify test assertion logic.
- These methods are intended exclusively for test scenarios. They may reference test-specific constraints (e.g., mocked service boundaries) that differ from production validation rules applied by the controller itself.
