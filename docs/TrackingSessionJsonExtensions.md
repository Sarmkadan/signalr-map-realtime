# TrackingSession JSON extensions

`TrackingSessionJsonExtensions` provides JSON serialization and deserialization helpers for `TrackingSession` using `System.Text.Json`.

## JSON configuration

All three methods share a single base `JsonSerializerOptions` instance configured with the web defaults and these explicit settings:

- Property names use camel case.
- Output is compact unless indentation is requested.
- Properties whose values are `null` are omitted.
- Reference cycles are ignored.
- Enum values are represented as camel-case strings.

## `ToJson`

```csharp
public static string ToJson(this TrackingSession value, bool indented = false)
```

Serializes a tracking session to JSON. By default, the result is compact. Passing `true` for `indented` creates a copy of the shared options, enables `WriteIndented` on that copy, and produces formatted output.

```csharp
string compactJson = session.ToJson();
string formattedJson = session.ToJson(indented: true);
```

## `FromJson`

```csharp
public static TrackingSession? FromJson(string json)
```

Deserializes JSON into a `TrackingSession`. A null or empty input is rejected by `ArgumentException.ThrowIfNullOrEmpty`. If `System.Text.Json` throws `JsonException`, the method catches it and returns `null`. A valid JSON `null` value can also deserialize to `null`.

```csharp
TrackingSession? session = TrackingSessionJsonExtensions.FromJson(json);

if (session is null)
{
    // The input was malformed or represented a JSON null value.
}
```

## `TryFromJson`

```csharp
public static bool TryFromJson(string json, out TrackingSession? value)
```

Attempts the same deserialization without allowing a `JsonException` to escape. A null or empty input is still rejected by `ArgumentException.ThrowIfNullOrEmpty`.

- On successful deserialization, it assigns the result to `value` and returns `true`.
- If deserialization throws `JsonException`, it assigns `null` to `value` and returns `false`.
- For the valid JSON literal `null`, deserialization succeeds, so the method returns `true` while `value` is `null`.

```csharp
if (TrackingSessionJsonExtensions.TryFromJson(json, out TrackingSession? session))
{
    // Deserialization completed. The result may still be null for JSON null.
}
```

## Error-handling distinction

`FromJson` and `TryFromJson` both convert malformed JSON into a non-throwing result. The difference is how they report it: `FromJson` returns `null`, whereas `TryFromJson` returns `false` and sets its output value to `null`. Because `FromJson` also returns `null` for a successfully parsed JSON `null`, use `TryFromJson` when the caller needs to distinguish malformed JSON from that valid JSON value.
