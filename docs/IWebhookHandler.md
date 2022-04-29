# IWebhookHandler

`IWebhookHandler` is an interface in the `signalr-map-realtime` project responsible for processing incoming webhook payloads, validating their authenticity, and extracting structured vehicle and delivery data for real-time mapping updates. It provides a standardized contract for handling webhook events related to vehicle positions, delivery statuses, and route optimizations.

## API

### `WebhookHandler`
A property or method (exact signature context unclear) that provides access to the underlying webhook handler implementation. The specific usage depends on the implementing class.

### `Task<WebhookProcessingResult> ProcessWebhookAsync`
Processes an incoming webhook payload asynchronously. Returns a `WebhookProcessingResult` indicating success or failure. Throws exceptions if the payload is malformed or processing fails due to internal errors.

### `bool ValidateSignature`
Validates the cryptographic signature of the webhook request to ensure authenticity. Returns `true` if valid; `false` otherwise. Does not throw exceptions.

### `bool Success`
Indicates whether the webhook was processed successfully. Read-only property.

### `string? ErrorMessage`
Contains error details if processing failed. Null if no error occurred.

### `DateTime? ProcessedAt`
Timestamp of when the webhook was processed. Null if processing has not completed.

### `Guid VehicleId`
Unique identifier of the vehicle associated with the webhook data.

### `double Latitude`
Geographic latitude coordinate of the vehicle's position.

### `double Longitude`
Geographic longitude coordinate of the vehicle's position.

### `double? Speed`
Vehicle speed in km/h. Null if speed data is unavailable.

### `double? Heading`
Direction of travel in degrees (0-360). Null if heading data is unavailable.

### `double? Accuracy`
Position accuracy in meters. Null if accuracy data is unavailable.

### `DateTime Timestamp`
Timestamp of the vehicle's reported position.

### `Guid DeliveryId`
Unique identifier of the delivery associated with the webhook.

### `string Status`
Current status of the delivery (e.g., "In Transit", "Delivered").

### `DateTime Timestamp`
Timestamp of the delivery status update.

### `Guid RouteId`
Unique identifier of the route associated with the delivery.

### `double OptimizationScore`
Score indicating route efficiency (0.0 to 1.0). Higher values indicate better optimization.

### `DateTime Timestamp`
Timestamp of the route optimization calculation.

## Usage

```csharp
// Example 1: Processing a webhook and checking result
var handler = serviceProvider.GetRequiredService<IWebhookHandler>();
var result = await handler.ProcessWebhookAsync(webhookPayload);

if (result.Success)
{
    Console.WriteLine($"Vehicle {result.VehicleId} at {result.Latitude}, {result.Longitude}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

```csharp
// Example 2: Validating webhook signature before processing
var isValid = handler.ValidateSignature(webhookHeaders, secretToken);
if (!isValid)
{
    throw new UnauthorizedAccessException("Invalid webhook signature");
}

var processed = await handler.ProcessWebhookAsync(payload);
```

## Notes

- `Speed`, `Heading`, and `Accuracy` may be null if the source data omits these fields; callers must handle null checks.
- Multiple `Timestamp` properties exist for different entities (vehicle position, delivery status, route optimization); ensure correct property usage based on context.
- Implementations should be thread-safe if used in concurrent environments, as `ProcessedAt` and `Success` may be accessed simultaneously.
- `ErrorMessage` provides diagnostic information only when `Success` is `false`; it should not be relied upon for business logic.
