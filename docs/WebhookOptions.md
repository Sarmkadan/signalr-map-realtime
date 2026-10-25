# WebhookOptions

`WebhookOptions` is a configuration class in the `SignalRMapRealtime.Configuration` namespace. It holds the per-provider timestamp tolerance used for replay-attack prevention on incoming webhooks. Values are bound from configuration under the `Webhooks` section (`WebhookOptions.SectionName`). Any setting that is not configured keeps the default shown below.

The class has no data-annotation attributes, so no validation is applied to its values at startup.

## Configuration section

```json
{
  "Webhooks": {
    "TimestampToleranceMinutes": {
      "tracking-service": 5,
      "notification-service": 10,
      "route-optimization": 15
    }
  }
}
```

## API

### `SectionName`
The configuration section name, `"Webhooks"`.

- **Type:** `const string`

### `TimestampToleranceMinutes`
Timestamp tolerance, in minutes, for each webhook provider. A value of `0` disables timestamp validation for that provider.

- **Type:** `Dictionary<string, int>`
- **Default:**
  - `tracking-service`: `5`
  - `notification-service`: `10`
  - `route-optimization`: `15`
- **Notes:** Keys are compared as case-sensitive strings, because the dictionary uses the default comparer. Write keys in lowercase so they match the lowercased provider name used by `GetTimestampToleranceMinutes`. Providers that are not listed have no entry and resolve to `0`.

### `GetTimestampToleranceMinutes(string provider)`
Returns the timestamp tolerance, in minutes, for the given provider.

- **Parameters:**
  - `provider` - the webhook provider name. The name is lowercased with `ToLowerInvariant()` before the lookup.
- **Returns:** `int` - the configured tolerance, or `0` if the provider is `null`, empty, or whitespace, or if it has no entry in `TimestampToleranceMinutes`.

```csharp
var options = new WebhookOptions();

options.GetTimestampToleranceMinutes("Tracking-Service"); // 5
options.GetTimestampToleranceMinutes("unknown-provider"); // 0
options.GetTimestampToleranceMinutes("   ");              // 0
```
