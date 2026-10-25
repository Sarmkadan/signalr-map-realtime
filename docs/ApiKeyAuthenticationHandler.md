# ApiKeyAuthenticationHandler

Source: `src/SignalRMapRealtime/Authentication/ApiKeyAuthenticationHandler.cs`

The file lives in the `SignalRMapRealtime.Authentication` namespace and defines one public type:

- `ApiKeyAuthenticationHandler` - an `AuthenticationHandler<ApiKeyAuthenticationOptions>` that authenticates requests by API key.

It depends on types defined elsewhere, which this document does not describe: `ApiKeyAuthenticationOptions` (with a `Keys` collection), `ApiKeyCredential` (with `Name` and `Scopes`), `ApiKeyVerifier.Find`, and `AuthenticationConstants` (header and query parameter names).

## Class overview

```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
```

The handler is constructed with three arguments:

| Parameter | Type | Use |
|-----------|------|-----|
| `options` | `IOptionsMonitor<ApiKeyAuthenticationOptions>` | Passed to the base class. The handler reads `Options.Keys`. |
| `logger` | `ILoggerFactory` | Checked with `ArgumentNullException.ThrowIfNull`. The handler creates its own `ILogger<ApiKeyAuthenticationHandler>` from it. |
| `encoder` | `UrlEncoder` | Passed to the base class. |

The XML doc on the class states that keys are matched against the SHA-256 hashes configured in `ApiKeyAuthenticationOptions.Keys`. This file does not hash anything itself. It passes the plaintext key to `ApiKeyVerifier.Find`, so the hashing and comparison happen there.

## Authentication flow

`HandleAuthenticateAsync` runs for each request and follows these steps:

1. **Extract the key.** Call `ExtractApiKey` (see below).
2. **Require a key.** If the result is `null` or empty, log a warning and return `AuthenticateResult.Fail` with a message that names the header, the query parameter, and the `access_token` parameter. The request is not authenticated.
3. **Find a matching credential.** Call `FindCredential(apiKey)`. If it returns `null`, log a warning ("Invalid API Key provided.") and return `AuthenticateResult.Fail("Invalid API Key.")`.
4. **Build the principal.** On a match:
   - Add `ClaimTypes.NameIdentifier` and `ClaimTypes.Name`, both set to `credential.Name`.
   - Add one `ClaimTypes.Role` claim per entry in `credential.Scopes`. Each value is passed through `Trim()`. Entries are not filtered, so a blank scope produces a role claim with an empty value.
   - Create a `ClaimsIdentity` with `Scheme.Name` as its authentication type, wrap it in a `ClaimsPrincipal`, and create an `AuthenticationTicket` with `Scheme.Name`.
5. **Log and succeed.** Log at information level with the key name and the scopes joined by `", "`, then return `AuthenticateResult.Success(ticket)`.

The log message in step 5 includes the key's name and roles, not the key itself. The invalid-key warning in step 3 does not log the presented key.

### Extracting the key

`ExtractApiKey` checks three sources in this order and returns the first one that is present:

1. The `X-API-Key` request header (`AuthenticationConstants.ApiKeyHeaderName`).
2. The `api_key` query parameter (`AuthenticationConstants.ApiKeyQueryParamName`).
3. The `access_token` query parameter.

The check is based on presence, not on content. If the header exists with an empty value, the method returns `""`, the query string is not checked, and step 2 fails. The same applies to `api_key` and `access_token`.

The `access_token` source is there because SignalR clients using WebSocket transport cannot send custom headers, so they pass the key in the query string instead. A key in the query string can end up in server, proxy, or access logs. This file does not address that.

### Validating the key

`FindCredential` runs these checks:

- If the key is null, empty, or whitespace-only, return `null`. A whitespace-only key passes the step 2 check (which uses `IsNullOrEmpty`) and then fails here with "Invalid API Key.".
- If `Options.Keys` has no entries, log an error that names the configuration path `SignalRMapRealtime:ApiKeyAuthentication:Keys` and return `null`. The request then fails with "Invalid API Key.".
- Otherwise, return `ApiKeyVerifier.Find(apiKey, Options.Keys)`. The XML doc describes this as a constant-time comparison over every configured key. The implementation is in `ApiKeyVerifier`, not in this file.

## Challenge and forbidden responses

The handler overrides two base methods to control the HTTP response. Both write a fixed JSON body, regardless of the failure reason from `AuthenticateResult.Fail`. The reason is not sent to the client.

| Method | Status | Content type | Body |
|--------|--------|--------------|------|
| `HandleChallengeAsync` | `401 Unauthorized` | `application/json` | `{"error":"Unauthorized","message":"Authentication required or failed."}` |
| `HandleForbiddenAsync` | `403 Forbidden` | `application/json` | `{"error":"Forbidden","message":"You do not have permission to access this resource."}` |

`HandleChallengeAsync` runs when the request needs authentication and none succeeded. `HandleForbiddenAsync` runs when the request is authenticated but the caller is not allowed. This handler does not check scopes itself. It only puts them into `ClaimTypes.Role` claims, so role-based policies must be configured elsewhere for a `403` to ever occur.

Both methods write the body with `CancellationToken.None`. They ignore the `AuthenticationProperties` argument.

## Logging summary

| Situation | Level | Message |
|-----------|-------|---------|
| No key in header or query | Warning | API Key not found in header ..., query parameter ..., or access_token parameter. |
| Key does not match | Warning | Invalid API Key provided. |
| No keys configured | Error | No API keys are configured. Add entries under SignalRMapRealtime:ApiKeyAuthentication:Keys. |
| Key matches | Information | API Key authenticated successfully for '{name}' with roles: {roles} |

The logger category is `ApiKeyAuthenticationHandler`.

## Notes

- The file does not show how the scheme is registered, what its name is, or which options are set. `Scheme.Name` is taken from the registration, not from this file.
- `HandleAuthenticateAsync` starts with a comment about a hotfix. It says that returning `401` for invalid keys stops clients from reconnecting in a loop. The code in the method is the behavior described here.
