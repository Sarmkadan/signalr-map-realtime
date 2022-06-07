# SignalrMapRealtimeOptionsTests

Overview of the test class that verifies the validation logic of `SignalrMapRealtimeOptions`. Each method represents a unit test that exercises a specific validation scenario, ensuring the options object correctly accepts or rejects configuration values.

## API

### Validate_WithValidConfiguration_ReturnsTrue
- **Purpose**: Confirms that when a fully valid `SignalrMapRealtimeOptions` instance is supplied, the `Validate` method returns `true`.
- **Parameters**: None.
- **Return Value**: `void` (the test asserts the expected result internally).
- **When it throws**: Throws an assertion exception (e.g., `AssertFailedException` or `Xunit.Sdk.EqualException`) if the validation does not return `true`.

### Validate_WithInvalidApiVersion_ReturnsFalse
- **Purpose**: Verifies that an options instance with an invalid API version string causes `Validate` to return `false`.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if the validation incorrectly returns `true`.

### Validate_WithInvalidEnvironment_ReturnsFalse
- **Purpose**: Ensures that setting an unsupported environment value results in `Validate` returning `false`.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if the validation incorrectly returns `true`.

### Validate_WithOutOfRangeValues_ReturnsFalse
- **Purpose**: Checks that numeric options outside their allowed ranges (e.g., negative timeouts) cause validation to fail.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if validation unexpectedly succeeds.

### AppInfoOptions_WithEmptyApiTitle_ReturnsFalse
- **Purpose**: Validates that an empty or whitespace‑only `ApiTitle` in the nested `AppInfoOptions` leads to a validation failure.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if validation passes incorrectly.

### SignalRHubsOptions_WithOutOfRangeMaxConnections_ReturnsFalse
- **Purpose**: Tests that specifying a `MaxConnections` value outside the permissible range for `SignalRHubsOptions` makes validation return `false`.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if validation incorrectly returns `true`.

### PerformanceOptions_WithOutOfRangeMaxConcurrentConnections_ReturnsFalse
- **Purpose**: Ensures that an out‑of‑range `MaxConcurrentConnections` value in `PerformanceOptions` triggers a validation failure.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if validation does not fail as expected.

### SectionName_ShouldBeCorrect
- **Purpose**: Asserts that the configuration section name used by `SignalrMapRealtimeOptions` matches the expected constant.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if the section name differs from the expected value.

### AllNestedOptions_ShouldHaveDefaultValues
- **Purpose**: Verifies that when a new `SignalrMapRealtimeOptions` instance is created, all nested options objects are initialized to their documented default values.
- **Parameters**: None.
- **Return Value**: `void`.
- **When it throws**: Throws an assertion exception if any nested option deviates from its default.

## Usage

The following examples show how the test methods can be invoked in a typical unit‑test project. They assume the test framework is xUnit; equivalent syntax applies to NUnit or MSTest with minor attribute changes.

```csharp
using Xunit;
using SignalrMapRealtime; // namespace containing SignalrMapRealtimeOptionsTests

public class SignalrMapRealtimeOptionsTestsFixture
{
    [Fact]
    public void ValidConfiguration_PassesValidation()
    {
        // Arrange – the test class contains the logic internally
        var test = new SignalrMapRealtimeOptionsTests();

        // Act & Assert – the method will throw if validation fails
        test.Validate_WithValidConfiguration_ReturnsTrue();
    }
}
```

```csharp
using NUnit.Framework;
using SignalrMapRealtime;

[TestFixture]
public class SignalrMapRealtimeOptionsTests_NUnit
{
    [Test]
    public void InvalidApiVersion_FailsValidation()
    {
        var test = new SignalrMapRealtimeOptionsTests();

        // The method asserts internally; NUnit treats any thrown exception as test failure
        Assert.DoesNotThrow(() => test.Validate_WithInvalidApiVersion_ReturnsFalse());
    }
}
```

## Notes

- **Edge Cases**: The tests cover boundary conditions such as empty strings, negative numbers, and values exceeding defined maxima. Passing `null` for any nested options object is not exercised by these methods; if the production code does not guard against null references, a `NullReferenceException` could arise.
- **Thread‑Safety**: Each test method operates on stateless data and does not modify shared resources. Consequently, the methods are safe to execute concurrently in parallel test runners without additional synchronization.
- **Exception Behavior**: As pure verification methods, they do not throw exceptions under normal circumstances; any exception raised originates from the underlying assertion framework and signals a test failure. Consumers should treat unexpected exceptions as indicative of a problem in the test implementation rather than the production code.
