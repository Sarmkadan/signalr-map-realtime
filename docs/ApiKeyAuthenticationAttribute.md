# ApiKeyAuthenticationAttribute

Source: `src/SignalRMapRealtime/Attributes/ApiKeyAuthenticationAttribute.cs`

The file lives in the `SignalRMapRealtime.Attributes` namespace and defines three public types:

- `ApiKeyAuthenticationAttribute` - an authorization filter that requires a valid API key.
- `ValidateModelStateAttribute` - an action filter that turns invalid model state into a `400` response.
- `AuthenticationAttributeExtensions` - a static class with one extension method, `RequireApiKey`, which has no effect.

## ApiKeyAuthenticationAttribute

Authorization filter that requires a valid API key. Apply it to a controller class or an action method.

```csharp
[ApiController]
[Route("api/[controller]")]
[ApiKeyAuthentication]
public class ExampleController : ControllerBase
{
    // All actions require an API key
}
```

**Declaration:** `public class ApiKeyAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter`  
**Targets:** `AttributeTargets.Class | AttributeTargets.Method`

### Public members

#### `OnAuthorizationAsync(AuthorizationFilterContext context)`
Runs the authorization check. Returns `Task`, and the body completes synchronously.

- Throws `ArgumentNullException` if `context` is `null`.
- Reads the key from the request (see below).
- If there is no key, sets `context.Result` to a `401` response and returns.
- If the key is not valid, sets `context.Result` to a `401` response and returns.
- If the key is valid, it does not set `context.Result`. The request continues to the action.

### Where the key is read

The key is read by a private method, `ExtractApiKey`, in this order:

1. The `X-API-Key` request header. If the header is present, its value is used, even when it is empty. The query string is then not checked.
2. The `api_key` query parameter, if the header is absent.
3. If neither is present, no key is found.

### Responses

| Condition | Status | Body |
|-----------|--------|------|
| No key found, or the key is an empty string | `401 Unauthorized` | `{ error = "API key is required", message = "Please provide an API key using 'X-API-Key' header or 'api_key' query parameter" }` |
| Key found, but not valid | `401 Unauthorized` | `{ error = "Invalid API key", message = "The provided API key is not valid" }` |
| Key valid | - | No result is set. The action runs. |

Both bodies are anonymous objects returned through `UnauthorizedObjectResult`.

A key that contains only whitespace is not caught by the "required" check, because that check uses `string.IsNullOrEmpty`. It reaches validation and gets the "Invalid API key" response.

### How the key is validated

The private static method `IsValidApiKey` does the check:

1. Returns `false` if the key is empty or whitespace.
2. Resolves `IOptionsMonitor<ApiKeyAuthenticationOptions>` from `HttpContext.RequestServices`.
3. Reads `Keys` from `Get(ApiKeyAuthenticationOptions.DefaultScheme)`.
4. Returns `true` when `ApiKeyVerifier.Find(apiKey, keys)` finds a match.

If `IOptionsMonitor<ApiKeyAuthenticationOptions>` is not registered in the service provider, `keys` is empty. Every request then fails with "Invalid API key". Per the method's summary comment, the match is made by hash in constant time. The file does not show how `ApiKeyVerifier` works.

### Side effects

The filter does not create a user, claims, or principal on success. Its code has a comment about adding claims, but no claims are added. Calling the filter does not change `HttpContext.User`.

## ValidateModelStateAttribute

Action filter that returns a consistent error response when model binding or validation fails.

**Declaration:** `public class ValidateModelStateAttribute : ActionFilterAttribute`

### `OnActionExecuting(ActionExecutingContext context)`
Runs before the action.

- If `context.ModelState` is invalid, it builds a dictionary. The keys are the names of the entries that have errors. The values are arrays of their error messages.
- It sets `context.Result` to `BadRequestObjectResult` with the body `ErrorResponse.ValidationError(errors, "Model validation failed")`.
- If the model state is valid, nothing changes.

```csharp
[ValidateModelState]
[HttpPost]
public IActionResult Create([FromBody] ExampleDto dto)
{
    // Runs only when the model state is valid
    return Ok();
}
```

This attribute does not authenticate requests. It is in this file for convenience.

## AuthenticationAttributeExtensions

Static class with an extension method for `ControllerBase`.

**Declaration:** `public static class AuthenticationAttributeExtensions`

### `RequireApiKey(this ControllerBase controller)`
The method body is empty. It does nothing, so calling it does not protect an action. Use `[ApiKeyAuthentication]` to enforce the requirement.

## Usage notes

- The attribute is enforced only where it is applied. An action on a class without the attribute is not protected by it.
- The `401` responses are sent by the filter before the action runs. The action is not reached.
- `OnAuthorizationAsync` is public because it is required by `IAsyncAuthorizationFilter`. The helper methods `ExtractApiKey` and `IsValidApiKey` are private.
