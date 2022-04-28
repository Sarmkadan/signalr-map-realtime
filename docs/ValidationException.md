# ValidationException

`ValidationException` is a custom exception type used to represent validation failures within the application. It encapsulates one or more validation error messages, enabling structured error reporting when input data or state does not conform to expected constraints.

## API

### Errors

`public IEnumerable<string> Errors { get; }`

A read-only collection of validation error messages. This property provides access to the individual errors that caused the exception to be thrown. The collection is immutable and reflects the errors present at the time the exception was instantiated.

### ValidationException()

`public ValidationException()`

Initializes a new instance of the `ValidationException` class with no error messages. This constructor is typically used when the exception is thrown without specific validation details, though it is more common to use parameterized constructors for meaningful error reporting.

### ValidationException(string message)

`public ValidationException(string message)`

Initializes a new instance of the `ValidationException` class with a single error message. The `message` parameter specifies the primary validation error description. This constructor is useful for simple validation scenarios where a single error needs to be communicated.

### ValidationException(IEnumerable<string> errors)

`public ValidationException(IEnumerable<string> errors)`

Initializes a new instance of the `ValidationException` class with a collection of error messages. The `errors` parameter accepts an enumerable of strings, each representing a distinct validation failure. This constructor is appropriate for complex validation scenarios involving multiple errors.

## Usage

```csharp
// Example 1: Throwing with a single error
if (string.IsNullOrEmpty(input))
{
    throw new ValidationException("Input cannot be null or empty.");
}
```

```csharp
// Example 2: Throwing with multiple errors
var errors = new List<string>();
if (age < 0)
    errors.Add("Age must be a positive number.");
if (string.IsNullOrEmpty(email))
    errors.Add("Email is required.");

if (errors.Any())
{
    throw new ValidationException(errors);
}
```

## Notes

- The `Errors` property is immutable after instantiation. Modifications to the source collection passed to the constructor do not affect the exception's internal state.
- All constructors ensure that `Errors` is never null. If no errors are provided, an empty enumerable is assigned.
- Instances of `ValidationException` are thread-safe for read operations, as the underlying error collection is immutable.
- When using the `IEnumerable<string>` constructor, the provided collection is enumerated immediately to capture the current state, preventing external modifications from affecting the exception.
