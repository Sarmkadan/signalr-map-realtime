# Asset JSON extensions

`AssetJsonExtensions` provides JSON serialization and deserialization helpers for `Asset` values using `System.Text.Json`.

## JSON settings

All methods use a shared set of web-oriented serializer options:

- Property names are written in camel case.
- Output is compact by default.
- Properties whose values are `null` are omitted.
- Reference cycles are ignored during serialization.
- Property-name matching is case-insensitive during deserialization.

## `ToJson`

```csharp
string json = asset.ToJson();
string readableJson = asset.ToJson(indented: true);
```

`ToJson` serializes an `Asset` to a JSON string. Passing `true` for `indented` creates a copy of the shared options with formatted output enabled; it does not alter the shared options used by later calls.

The method throws `ArgumentNullException` when the asset is `null`. Serialization errors from `System.Text.Json` are not caught.

## `FromJson`

```csharp
Asset? asset = AssetJsonExtensions.FromJson(json);
```

`FromJson` deserializes a JSON string into an `Asset`. It returns `null` when the input is `null`, empty, or consists only of whitespace, or when deserialization produces a null result. Deserialization exceptions are not caught, including `JsonException` for invalid JSON.

## `TryFromJson`

```csharp
if (AssetJsonExtensions.TryFromJson(json, out Asset? asset))
{
    // asset is non-null here.
}
```

`TryFromJson` returns `true` only when deserialization completes and produces a non-null `Asset`. It returns `false` and sets `value` to `null` for:

- Empty or whitespace-only input.
- Invalid JSON.
- JSON that deserializes to `null`.

A `null` input throws `ArgumentNullException`. The method catches `JsonException` only; other exceptions are allowed to propagate.
