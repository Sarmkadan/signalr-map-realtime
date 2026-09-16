# VehicleRepository JSON extensions

`VehicleRepositoryJsonExtensions` provides JSON serialization and deserialization helpers for `VehicleRepository` using `System.Text.Json`.

## JSON configuration

All methods share serializer options created with `JsonSerializerDefaults.Web` and the following explicit settings:

- Property names are written using camel case.
- JSON is compact by default.
- Properties whose values are `null` are omitted when serializing.
- Reference cycles are ignored.

Deserialization uses the same options.

## `ToJson`

```csharp
string json = repository.ToJson();
string readableJson = repository.ToJson(indented: true);
```

`ToJson` is an extension method on `VehicleRepository`. It serializes the repository to a JSON string. Passing `true` for `indented` creates a copy of the shared options with indentation enabled; it does not change the shared configuration used by other calls.

If `value` is `null`, the method throws `ArgumentNullException`. Serialization failures from `System.Text.Json` are allowed to propagate.

## `FromJson`

```csharp
VehicleRepository? repository = VehicleRepositoryJsonExtensions.FromJson(json);
```

`FromJson` deserializes JSON into a `VehicleRepository`. It returns the value produced by `JsonSerializer.Deserialize`, which may be `null` when the JSON token is `null`.

Despite its nullable return type, null, empty, and whitespace-only input is rejected by `ArgumentException.ThrowIfNullOrWhiteSpace`: null input throws `ArgumentNullException`, while empty or whitespace-only input throws `ArgumentException`. Invalid JSON or JSON that cannot be deserialized throws `JsonException`.

## `TryFromJson`

```csharp
if (VehicleRepositoryJsonExtensions.TryFromJson(json, out VehicleRepository? repository))
{
    // repository is non-null
}
```

`TryFromJson` deserializes JSON and assigns the result to the `out` parameter. It returns `true` when that result is non-null and `false` when deserialization produces `null`.

The method is not an exception-catching `Try` API. Null, empty, and whitespace-only input throws as described for `FromJson`, and malformed or otherwise invalid JSON can throw `JsonException`.

## Option reuse

Compact serialization and all deserialization calls reuse a private, read-only `JsonSerializerOptions` instance. Indented serialization uses a per-call copy, leaving the shared options unchanged.
