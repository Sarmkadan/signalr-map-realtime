# ClusteringControllerJsonExtensions

`ClusteringControllerJsonExtensions` provides `System.Text.Json` serialization and deserialization helpers for `ClusteringController`.

## Serializer configuration

All methods use a shared `JsonSerializerOptions` instance initialized with `JsonSerializerDefaults.Web`. The configuration:

- uses camel case property names;
- writes compact JSON by default;
- omits properties whose values are `null`;
- serializes enum values as camel case strings.

The shared options are exposed through the read-only `JsonSerializerOptions` property. This property returns the same mutable options instance used by the extension methods.

## API

### `ToJson`

```csharp
string ToJson(this ClusteringController value, bool indented = false)
```

Serializes a controller to JSON. Compact output is the default. When `indented` is `true`, the method creates a copy of the shared options, enables indentation on that copy, and serializes with it.

The method throws `ArgumentNullException` when `value` is `null`. Serialization exceptions from `JsonSerializer.Serialize` are not caught.

Example:

```csharp
string compactJson = controller.ToJson();
string readableJson = controller.ToJson(indented: true);
```

### `FromJson`

```csharp
ClusteringController? FromJson(string json)
```

Deserializes JSON with the shared serializer options and returns the resulting controller, which may be `null`.

The method throws `ArgumentNullException` when `json` is `null`. Deserialization exceptions are not caught.

Example:

```csharp
ClusteringController? controller =
    ClusteringControllerJsonExtensions.FromJson(json);
```

### `TryFromJson`

```csharp
bool TryFromJson(string json, out ClusteringController? value)
```

Attempts to deserialize JSON with the shared serializer options. It returns `true` when `JsonSerializer.Deserialize` completes without throwing, including when the deserialized value is `null`. If deserialization throws `JsonException`, the method sets `value` to `null` and returns `false`.

The method throws `ArgumentNullException` when `json` is `null`. It catches only `JsonException`; other exception types propagate to the caller.

Example:

```csharp
if (ClusteringControllerJsonExtensions.TryFromJson(json, out var controller))
{
    // Deserialization completed; controller may still be null.
}
```

## Behavioral notes

- `ToJson(indented: true)` does not change the shared options because indentation is enabled on a copied options instance.
- `FromJson` and `TryFromJson` are static helpers rather than extension methods; `ToJson` is an extension method on `ClusteringController`.
- `TryFromJson` reports whether deserialization threw a `JsonException`, not whether the returned controller is non-null.
