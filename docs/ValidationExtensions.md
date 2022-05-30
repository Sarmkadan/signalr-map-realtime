# ValidationExtensions

A static utility class providing common validation methods for strings, numeric types, collections, and other data formats. Designed for use in real-time SignalR applications where input validation is critical for security and correctness.

## API

### `public static bool IsValidEmail(string? input)`

Validates whether the input string is a syntactically valid email address. Supports standard email formats including subdomains and special characters.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input matches a valid email pattern; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---

### `public static bool IsValidPhoneNumber(string? input)`

Validates whether the input string resembles a valid phone number. Accepts international formats with optional `+`, spaces, dashes, and parentheses.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input matches a common phone number pattern; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---

### `public static bool IsValidUrl(string? input)`

Validates whether the input string is a syntactically valid URL. Supports `http`, `https`, and optional ports or paths.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input matches a valid URL pattern; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsValidIpAddress(string? input)`

Validates whether the input string is a valid IPv4 or IPv6 address.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input is a valid IP address; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsValidGuid(string? input)`

Validates whether the input string is a valid GUID (Globally Unique Identifier), including formats with or without hyphens.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input matches a valid GUID pattern; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsAlphanumeric(string? input)`

Validates whether the input string contains only alphanumeric characters (letters and digits). Whitespace or special characters cause validation to fail.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input contains only alphanumeric characters; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsStrongPassword(string? input)`

Validates whether the input string meets basic password strength criteria: at least 8 characters, containing at least one uppercase letter, one lowercase letter, one digit, and one special character.

- **Parameters**:
  - `input` (string?): The string to validate. Null or empty strings return `false`.
- **Returns**: `true` if the input meets the password strength criteria; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsInRange<T>(T value, T min, T max)`

Validates whether a numeric value lies within a specified inclusive range. Supports all numeric types (`int`, `double`, `decimal`, etc.) that implement `IComparable<T>`.

- **Parameters**:
  - `value` (T): The value to check.
  - `min` (T): The lower bound (inclusive).
  - `max` (T): The upper bound (inclusive).
- **Returns**: `true` if `min <= value <= max`; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsLengthInRange(string? input, int minLength, int maxLength)`

Validates whether the length of the input string is within the specified inclusive range.

- **Parameters**:
  - `input` (string?): The string to validate. Null returns `false`.
  - `minLength` (int): The minimum allowed length (inclusive).
  - `maxLength` (int): The maximum allowed length (inclusive).
- **Returns**: `true` if `minLength <= input.Length <= maxLength`; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool HasElements<T>(IEnumerable<T>? collection)`

Validates whether the collection is non-null and contains at least one element.

- **Parameters**:
  - `collection` (IEnumerable<T>?): The collection to validate.
- **Returns**: `true` if the collection is non-null and has one or more elements; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool HasExactly<T>(IEnumerable<T>? collection, int count)`

Validates whether the collection is non-null and contains exactly the specified number of elements.

- **Parameters**:
  - `collection` (IEnumerable<T>?): The collection to validate.
  - `count` (int): The exact number of elements required.
- **Returns**: `true` if the collection is non-null and has exactly `count` elements; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool HasAtLeast<T>(IEnumerable<T>? collection, int minCount)`

Validates whether the collection is non-null and contains at least the specified number of elements.

- **Parameters**:
  - `collection` (IEnumerable<T>?): The collection to validate.
  - `minCount` (int): The minimum number of elements required (inclusive).
- **Returns**: `true` if the collection is non-null and has at least `minCount` elements; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsPositive<T>(T value)`

Validates whether a numeric value is strictly greater than zero. Supports all numeric types that implement `IComparable<T>`.

- **Parameters**:
  - `value` (T): The value to check.
- **Returns**: `true` if `value > 0`; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsNegative<T>(T value)`

Validates whether a numeric value is strictly less than zero. Supports all numeric types that implement `IComparable<T>`.

- **Parameters**:
  - `value` (T): The value to check.
- **Returns**: `true` if `value < 0`; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsNaN(double value)`

Validates whether a `double` value is not-a-number (NaN).

- **Parameters**:
  - `value` (double): The value to check.
- **Returns**: `true` if `value` is NaN; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsInfinite(double value)`

Validates whether a `double` value is infinite (either positive or negative infinity).

- **Parameters**:
  - `value` (double): The value to check.
- **Returns**: `true` if `value` is infinite; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool IsValidPercentage(double value)`

Validates whether a numeric value is a valid percentage, i.e., between 0.0 and 100.0 inclusive.

- **Parameters**:
  - `value` (double): The value to check.
- **Returns**: `true` if `0.0 <= value <= 100.0`; otherwise, `false`.
- **Throws**: Does not throw exceptions.

---
### `public static bool MatchesPattern(string? input, string pattern)`

Validates whether the input string matches the specified regular expression pattern.

- **Parameters**:
  - `input` (string?): The string to validate. Null returns `false`.
  - `pattern` (string): The regular expression pattern to match against.
- **Returns**: `true` if the input matches the pattern; otherwise, `false`.
- **Throws**: Does not throw exceptions. Invalid regex patterns are not handled—assumes pattern is valid.

## Usage
