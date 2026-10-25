# AuthenticationConstants

Source: `src/SignalRMapRealtime/Authentication/AuthenticationConstants.cs`

The file lives in the `SignalRMapRealtime.Authentication` namespace and defines the static class `AuthenticationConstants`. It contains three `const string` fields that name the API Key authentication scheme, the HTTP header, and the query parameter used to carry the API key. The class cannot be instantiated and needs no configuration.

## AuthenticationConstants

Static class with the names used by API Key authentication.

### Scheme

#### `ApiKeySchemeName`
The name of the API Key authentication scheme.  
**Type:** `string`  
**Value:** `"ApiKey"`

### Transport

#### `ApiKeyHeaderName`
The name of the HTTP header where the API key is expected.  
**Type:** `string`  
**Value:** `"X-API-Key"`

#### `ApiKeyQueryParamName`
The name of the query parameter where the API key is expected.  
**Type:** `string`  
**Value:** `"api_key"`

## Usage notes

- All members are `const`, so their values are fixed at compile time and are inlined by the compiler.
- The file defines the names only. It does not read, validate, or store any API key.
- Because the values are compile-time constants, they can be used where a constant is required, such as attribute arguments.
