# StringExtensions

`StringExtensions` is a static utility class that provides a collection of extension methods for the `System.String` type. It offers common string inspection, transformation, truncation, and masking operations that are not present in the base class library, enabling more expressive and concise string handling throughout the `signalr-map-realtime` project.

## API

### `IsNullOrEmpty`

```csharp
public static bool IsNullOrEmpty(this string value)
```

Determines whether the string instance is `null` or equal to `String.Empty`.

| Parameter | Type     | Description              |
| --------- | -------- | ------------------------ |
| `value`   | `string` | The string to evaluate.  |

**Returns:** `true` if the string is `null` or has a length of zero; otherwise `false`.

**Throws:** This method does not throw.

---

### `HasValue`

```csharp
public static bool HasValue(this string value)
```

Determines whether the string instance is neither `null` nor empty.

| Parameter | Type     | Description              |
| --------- | -------- | ------------------------ |
| `value`   | `string` | The string to evaluate.  |

**Returns:** `true` if the string is not `null` and has a length greater than zero; otherwise `false`.

**Throws:** This method does not throw.

---

### `Truncate`

```csharp
public static string Truncate(this string value, int maxLength)
```

Truncates the string to the specified maximum length. If the string is shorter than or equal to `maxLength`, it is returned unchanged.

| Parameter   | Type     | Description                                      |
| ----------- | -------- | ------------------------------------------------ |
| `value`     | `string` | The string to truncate.                          |
| `maxLength` | `int`    | The maximum number of characters to retain.      |

**Returns:** A new string containing the first `maxLength` characters of the original string, or the original string if its length does not exceed `maxLength`.

**Throws:** `ArgumentOutOfRangeException` when `maxLength` is negative.

---

### `ToTitleCase`

```csharp
public static string ToTitleCase(this string value)
```

Converts the string to title case, where the first character of each word is uppercase and the remaining characters are lowercase. Word boundaries are determined by whitespace.

| Parameter | Type     | Description                    |
| --------- | -------- | ------------------------------ |
| `value`   | `string` | The string to convert.         |

**Returns:** A new string in title case, or `String.Empty` if the input is `null` or empty.

**Throws:** This method does not throw.

---

### `ToKebabCase`

```csharp
public static string ToKebabCase(this string value)
```

Converts the string to kebab-case (lowercase words separated by hyphens). Sequences of uppercase letters, digits, and transitions between lowercase and uppercase are used to infer word boundaries.

| Parameter | Type     | Description                    |
| --------- | -------- | ------------------------------ |
| `value`   | `string` | The string to convert.         |

**Returns:** A new kebab-case string, or `String.Empty` if the input is `null` or empty.

**Throws:** This method does not throw.

---

### `ToSnakeCase`

```csharp
public static string ToSnakeCase(this string value)
```

Converts the string to snake_case (lowercase words separated by underscores). Word boundaries are inferred using the same logic as `ToKebabCase`.

| Parameter | Type     | Description                    |
| --------- | -------- | ------------------------------ |
| `value`   | `string` | The string to convert.         |

**Returns:** A new snake_case string, or `String.Empty` if the input is `null` or empty.

**Throws:** This method does not throw.

---

### `SubstringSafe`

```csharp
public static string SubstringSafe(this string value, int startIndex, int length)
```

Safely extracts a substring without throwing when the requested range exceeds the string bounds. If `startIndex` is beyond the end of the string, an empty string is returned. If `startIndex + length` exceeds the available characters, the substring is truncated to the end of the string.

| Parameter    | Type     | Description                                          |
| ------------ | -------- | ---------------------------------------------------- |
| `value`      | `string` | The string to extract from.                          |
| `startIndex` | `int`    | The zero-based starting character position.          |
| `length`     | `int`    | The desired number of characters.                    |

**Returns:** The substring within the safe bounds, or `String.Empty` if the input is `null` or `startIndex` is out of range.

**Throws:** This method does not throw.

---

### `RemoveCharacters`

```csharp
public static string RemoveCharacters(this string value, params char[] characters)
```

Removes all occurrences of the specified characters from the string.

| Parameter    | Type     | Description                                      |
| ------------ | -------- | ------------------------------------------------ |
| `value`      | `string` | The string to filter.                            |
| `characters` | `char[]` | The characters to remove.                        |

**Returns:** A new string with the specified characters removed, or `String.Empty` if the input is `null`.

**Throws:** This method does not throw.

---

### `CountOccurrences`

```csharp
public static int CountOccurrences(this string value, string substring, StringComparison comparisonType)
```

Counts the number of non-overlapping occurrences of a substring within the string using the specified comparison rules.

| Parameter        | Type              | Description                                          |
| ---------------- | ----------------- | ---------------------------------------------------- |
| `value`          | `string`          | The string to search.                                |
| `substring`      | `string`          | The substring to count.                              |
| `comparisonType` | `StringComparison` | The culture and case rules for comparison.         |

**Returns:** The number of times `substring` appears in the string. Returns `0` if the input is `null` or empty, or if `substring` is `null` or empty.

**Throws:** This method does not throw.

---

### `Reverse`

```csharp
public static string Reverse(this string value)
```

Reverses the order of characters in the string.

| Parameter | Type     | Description                    |
| --------- | -------- | ------------------------------ |
| `value`   | `string` | The string to reverse.         |

**Returns:** A new string with characters in reverse order, or `String.Empty` if the input is `null`.

**Throws:** This method does not throw.

---

### `Repeat`

```csharp
public static string Repeat(this string value, int count)
```

Returns a new string that repeats the original string a specified number of times.

| Parameter | Type     | Description                                      |
| --------- | -------- | ------------------------------------------------ |
| `value`   | `string` | The string to repeat.                            |
| `count`   | `int`    | The number of repetitions.                       |

**Returns:** A concatenated string consisting of `count` copies of `value`. Returns `String.Empty` if `value` is `null` or empty, or if `count` is zero.

**Throws:** `ArgumentOutOfRangeException` when `count` is negative.

---

### `Matches`

```csharp
public static bool Matches(this string value, string pattern, StringComparison comparisonType)
```

Determines whether the string matches a specified pattern using wildcard characters `*` (zero or more characters) and `?` (exactly one character).

| Parameter        | Type              | Description                                          |
| ---------------- | ----------------- | ---------------------------------------------------- |
| `value`          | `string`          | The string to evaluate.                              |
| `pattern`        | `string`          | The wildcard pattern to match against.               |
| `comparisonType` | `StringComparison` | The culture and case rules for comparison.         |

**Returns:** `true` if the string matches the pattern; otherwise `false`. Returns `false` if `value` is `null` or `pattern` is `null`.

**Throws:** This method does not throw.

---

### `Mask`

```csharp
public static string Mask(this string value, int visibleStart, int visibleEnd, char maskChar = '*')
```

Masks a portion of the string by replacing characters between the visible prefix and suffix with a masking character.

| Parameter      | Type     | Description                                                      |
| -------------- | -------- | ---------------------------------------------------------------- |
| `value`        | `string` | The string to mask.                                              |
| `visibleStart` | `int`    | Number of characters to leave unmasked at the beginning.         |
| `visibleEnd`   | `int`    | Number of characters to leave unmasked at the end.               |
| `maskChar`     | `char`   | The character used for masking (default `'*'`).                  |

**Returns:** A new string with the middle portion masked, or the original string if it is shorter than `visibleStart + visibleEnd`. Returns `String.Empty` if the input is `null`.

**Throws:** `ArgumentOutOfRangeException` when `visibleStart` or `visibleEnd` is negative.

## Usage

### Example 1: Sanitizing and formatting user input for display

```csharp
string rawInput = "  JOHN DOE 123 Main St.  ";
string sanitized = rawInput
    .RemoveCharacters('.')
    .Trim()
    .ToTitleCase();

bool hasContent = sanitized.HasValue(); // true
string displayName = sanitized.Truncate(20); // "John Doe 123 Main St"

Console.WriteLine(displayName);
```

### Example 2: Generating a masked identifier with a kebab-case slug

```csharp
string identifier = "CustomerAccount_12345";
string slug = identifier.ToKebabCase(); // "customer-account-12345"
string maskedId = identifier.Mask(visibleStart: 4, visibleEnd: 4, maskChar: '#');
// "Cust#############2345"

bool matchesPattern = slug.Matches("customer-*", StringComparison.OrdinalIgnoreCase); // true

Console.WriteLine($"Slug: {slug}, Masked: {maskedId}, Matches: {matchesPattern}");
```

## Notes

- All methods treat `null` input gracefully, typically returning `false`, `0`, or `String.Empty` rather than throwing `NullReferenceException`. Callers can safely chain these extension methods without explicit null guards.
- `ToKebabCase` and `ToSnakeCase` rely on heuristics for word-boundary detection (uppercase-to-lowercase transitions, consecutive uppercase sequences, and digit boundaries). Strings in all-uppercase with no separators (e.g., `"ABCD"`) will be treated as a single word.
- `SubstringSafe` is designed for scenarios where substring ranges are computed dynamically and may exceed string length; it avoids the `ArgumentOutOfRangeException` that `String.Substring` would throw.
- `Matches` uses `*` and `?` as the only wildcard tokens. It does not support character classes or escaping. Patterns containing other characters are matched literally.
- `Mask` returns the original string unchanged when `visibleStart + visibleEnd` is greater than or equal to the string length. No masking character is inserted in that case.
- These methods are static extension methods and are inherently stateless. They do not mutate the original string (strings are immutable) and do not access shared mutable state, making them safe for concurrent use from multiple threads without synchronization.
