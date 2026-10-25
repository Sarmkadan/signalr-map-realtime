# WebhooksController

The `WebhooksController` receives inbound webhook calls from three external services (tracking, notification, and route optimization). Every request is authenticated with an HMAC-SHA256 signature before the payload is touched, and the timestamp is checked against a configurable tolerance window to reject replays. Validated payloads are handed to `IWebhookHandler`.

The controller is declared with `[ApiController]` and `[Route("api/[controller]")]`, so all endpoints live under `/api/webhooks`.

## Configuration

Each provider needs a shared secret, read from `IConfiguration`:

| Provider | Route | Signature header | Secret key |
|---|---|---|---|
| Tracking service | `POST /api/webhooks/tracking-service` | `X-Signature` | `Webhooks:TrackingService:Secret` |
| Notification service | `POST /api/webhooks/notification-service` | `X-Webhook-Secret` | `Webhooks:NotificationService:Secret` |
| Route optimization | `POST /api/webhooks/route-optimization` | `Authorization: Bearer <signature>` | `Webhooks:RouteOptimization:Secret` |

The timestamp tolerance is supplied by `WebhookOptions.GetTimestampToleranceMinutes(provider)`. A value of `0` or less disables timestamp validation for that provider.

If a provider's secret is missing or empty, every request from that provider is rejected with `401 Unauthorized` and a warning is logged.

## API

### `public WebhooksController(IWebhookHandler, ILogger<WebhooksController>, IOptions<WebhookOptions>, IConfiguration)`
Initializes the controller with its dependencies. Each argument is checked with `ArgumentNullException.ThrowIfNull`, so a `null` dependency throws at construction time. `IOptions<WebhookOptions>.Value` is captured once in the constructor.

### `public async Task<IActionResult> Track([FromBody] string payload)`
Handles `POST /api/webhooks/tracking-service`. Accepts `Content-Type: application/json` (enforced by `[Consumes]`). Delegates to the shared processing pipeline using the provider name `"tracking-service"`.
*   **Request**: The raw JSON body bound as a `string`, plus the `X-Signature` header and optionally `X-Timestamp`.
*   **Responses**: `200 OK`, `400 Bad Request`, `401 Unauthorized` (see [Response Shapes](#response-shapes)).

### `public async Task<IActionResult> Notify([FromBody] string payload)`
Handles `POST /api/webhooks/notification-service`. Same behavior as `Track`, using the provider name `"notification-service"` and the `X-Webhook-Secret` header for the signature.
*   **Request**: The raw JSON body bound as a `string`, plus the `X-Webhook-Secret` header and optionally `X-Timestamp`.
*   **Responses**: `200 OK`, `400 Bad Request`, `401 Unauthorized`.

### `public async Task<IActionResult> Optimize([FromBody] string payload)`
Handles `POST /api/webhooks/route-optimization`. Same behavior as `Track`, using the provider name `"route-optimization"`. The signature is read from the `Authorization` header.
*   **Request**: The raw JSON body bound as a `string`, plus an `Authorization: Bearer <signature>` header and optionally `X-Timestamp`.
*   **Responses**: `200 OK`, `400 Bad Request`, `401 Unauthorized`.

### `private async Task<IActionResult> ProcessWebhookAsync(string provider, string payload)`
Shared pipeline behind all three endpoints. Runs the steps below in order and stops at the first failure:

1.  **Signature check** (`ValidateSignature`). On failure, logs a warning and returns `401` with `{ "error": "Invalid signature" }`. Nothing is deserialized before this step.
2.  **Timestamp check** (`ValidateTimestamp`). On failure, logs a warning and returns `401` with `{ "error": "Request too old or timestamp invalid" }`.
3.  **Handler call**. Invokes `IWebhookHandler.ProcessWebhookAsync(provider, payload, headers)`, passing all request headers as a case-insensitive dictionary.
4.  **Result mapping**. A successful result returns `200 OK`. An unsuccessful result returns `400` with the handler's error message, or `"Processing failed"` if none was provided.
5.  **Exception guard**. Any unhandled exception is logged and converted to `400` with `{ "error": "Internal server error" }`.

### `private bool ValidateSignature(string provider, string payload)`
Dispatches to the provider-specific validator based on the lower-cased provider name. An unknown provider returns `false`. Throws `ArgumentException` if `provider` is null or empty, and `ArgumentNullException` if `payload` is null.

### `private bool ValidateTrackingServiceSignature(string payload)`
Reads `X-Signature`. Returns `false` if the header is absent or empty. Otherwise verifies the first header value against the tracking-service secret.

### `private bool ValidateNotificationServiceSignature(string payload)`
Reads `X-Webhook-Secret`. Returns `false` if the header is absent or empty. Otherwise verifies the first header value against the notification-service secret.

### `private bool ValidateRouteOptimizationSignature(string payload)`
Reads `Authorization`. Returns `false` if the header is absent or empty, or if its value is blank or does not start with `Bearer ` (case-insensitive, trailing space required). The text after `Bearer ` is treated as the signature and verified against the route-optimization secret.

### `private bool ValidateTimestamp(string provider)`
Enforces the replay window.
*   If the provider's tolerance is `<= 0`, returns `true` without checking anything.
*   Otherwise, requires an `X-Timestamp` header containing a Unix time in whole seconds (parsed with `long.TryParse`). A missing or non-numeric value returns `false`.
*   Returns `false` if the timestamp differs from the current UTC time by more than the tolerance in either direction (past or future).

### `private bool VerifyHmacSignature(string payload, string providedSignature, string? secret)`
Core verification routine.
*   Returns `false` and logs a warning if `secret` is null, empty, or whitespace.
*   Strips an optional case-insensitive `sha256=` prefix.
*   Decodes the remainder with `Convert.FromHexString` after trimming whitespace. Hex decoding is case-insensitive. Invalid hex returns `false`.
*   Computes HMAC-SHA256 over the UTF-8 bytes of `payload` using the UTF-8 bytes of `secret` as the key.
*   Compares the result with `CryptographicOperations.FixedTimeEquals`, so timing does not reveal how many bytes matched.

### `private Dictionary<string, string> GetHeadersDictionary()`
Copies every request header into a new case-insensitive dictionary for the handler. Header values are converted from `StringValues` to `string`.

## Response Shapes

All responses are anonymous JSON objects produced by `Ok(...)`, `BadRequest(...)`, and `Unauthorized(...)`. The controller does not use `ApiResponse<T>` or `ErrorResponse`.

**200 OK**: handler succeeded
```json
{
  "success": true,
  "processedAt": "<value of IWebhookHandler result.ProcessedAt>"
}
```

**400 Bad Request**: handler reported failure, or an exception was thrown
```json
{ "error": "<result.ErrorMessage, or 'Processing failed'>" }
```
```json
{ "error": "Internal server error" }
```

**401 Unauthorized**: signature or timestamp check failed
```json
{ "error": "Invalid signature" }
```
```json
{ "error": "Request too old or timestamp invalid" }
```

The `[ProducesResponseType]` attributes on each endpoint declare `200`, `400`, and `401`.

## Error Handling

*   **Validation before processing.** Signature and timestamp checks run before the payload is passed to the handler, so unauthenticated requests never reach `IWebhookHandler`.
*   **Signature failures return `401`.** This covers a missing header, a missing or empty secret, malformed hex, a wrong signature, and an unknown provider. All of them return the same `"Invalid signature"` message, so callers cannot distinguish the cause from the response.
*   **Timestamp failures return `401`.** This covers a missing `X-Timestamp`, a non-numeric value, and a value outside the tolerance window.
*   **Handler failures return `400`.** The handler's `ErrorMessage` is returned to the caller when present.
*   **Unexpected exceptions return `400`, not `500`.** Any exception in the pipeline is logged with the provider name and converted to `400` with a generic message. The original exception is not returned to the caller.
*   **Logging.** Failed signature checks are logged at `Warning`, missing or malformed headers at `Debug`, handler failures at `Error`, and successful processing at `Information`.

## Usage

### Example 1: Signing a tracking-service request

The signature is HMAC-SHA256 over the exact request body, encoded as lowercase hex. The `sha256=` prefix is optional.

```bash
BODY='{"vehicleId":"42","lat":50.45,"lng":30.52}'
SIG=$(printf '%s' "$BODY" | openssl dgst -sha256 -hmac "$TRACKING_SECRET" | awk '{print $2}')

curl -X POST https://localhost:5001/api/webhooks/tracking-service \
  -H "Content-Type: application/json" \
  -H "X-Signature: sha256=$SIG" \
  -H "X-Timestamp: $(date +%s)" \
  --data-binary "$BODY"
```

A successful call returns `200 OK` with `{"success":true,"processedAt":...}`.

### Example 2: Route optimization with a Bearer-formatted signature

```bash
SIG=$(printf '%s' "$BODY" | openssl dgst -sha256 -hmac "$ROUTE_SECRET" | awk '{print $2}')

curl -X POST https://localhost:5001/api/webhooks/route-optimization \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $SIG" \
  -H "X-Timestamp: $(date +%s)" \
  --data-binary "$BODY"
```

### Notes for integrators

*   Sign the body bytes exactly as sent. Any reformatting, such as pretty-printing or changing whitespace, invalidates the signature.
*   `X-Timestamp` is checked for freshness but is **not** included in the HMAC input. The signature covers only the payload, so a captured request can be replayed with a new timestamp within the tolerance window.
*   If timestamp validation is disabled for a provider (tolerance `<= 0`), replay protection relies solely on the signature.
