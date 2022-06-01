# ClaimsExtensions

Extension methods for extracting common claims from a `ClaimsPrincipal` in SignalR real-time applications. These utilities simplify access to user identity data such as identifiers, names, roles, and organizational context, reducing boilerplate when working with claims-based authentication in SignalR hubs or message handlers.

## API

### `GetUserId`
Extracts the user identifier claim (`ClaimTypes.NameIdentifier`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose identifier is to be retrieved.
- **Return value**: `string?` – the value of the identifier claim, or `null` if the principal is unauthenticated or the claim is missing.
- **Exceptions**: None.

### `GetUserName`
Extracts the name claim (`ClaimTypes.Name`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose name is to be retrieved.
- **Return value**: `string?` – the value of the name claim, or `null` if the principal is unauthenticated or the claim is missing.
- **Exceptions**: None.

### `GetUserEmail`
Extracts the email claim (`ClaimTypes.Email`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose email is to be retrieved.
- **Return value**: `string?` – the value of the email claim, or `null` if the principal is unauthenticated or the claim is missing.
- **Exceptions**: None.

### `GetUserRoles`
Extracts all role claims (`ClaimTypes.Role`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose roles are to be retrieved.
- **Return value**: `IEnumerable<string>` – an enumerable of role values; empty if the principal is unauthenticated or has no roles.
- **Exceptions**: None.

### `HasRole`
Determines whether the principal has a specific role.

- **Parameters**:
  - `ClaimsPrincipal principal` – the user to check.
  - `string role` – the role to match.
- **Return value**: `bool` – `true` if the principal has the role; otherwise, `false`.
- **Exceptions**: None.

### `HasAnyRole`
Determines whether the principal has any of the specified roles.

- **Parameters**:
  - `ClaimsPrincipal principal` – the user to check.
  - `IEnumerable<string> roles` – the roles to match against.
- **Return value**: `bool` – `true` if the principal has at least one of the roles; otherwise, `false`.
- **Exceptions**: None.

### `GetClaimValue`
Extracts the value of a claim with the specified type.

- **Parameters**:
  - `ClaimsPrincipal principal` – the user whose claim is to be retrieved.
  - `string claimType` – the type of the claim to find.
- **Return value**: `string?` – the value of the first matching claim, or `null` if no claim of the given type exists or the principal is unauthenticated.
- **Exceptions**: None.

### `GetClaimValues`
Extracts all values of claims with the specified type.

- **Parameters**:
  - `ClaimsPrincipal principal` – the user whose claims are to be retrieved.
  - `string claimType` – the type of the claims to find.
- **Return value**: `IEnumerable<string>` – an enumerable of values for claims of the given type; empty if no matching claims exist or the principal is unauthenticated.
- **Exceptions**: None.

### `HasClaim`
Determines whether the principal has a claim of the specified type.

- **Parameters**:
  - `ClaimsPrincipal principal` – the user to check.
  - `string claimType` – the type of the claim to find.
- **Return value**: `bool` – `true` if at least one claim of the given type exists; otherwise, `false`.
- **Exceptions**: None.

### `GetOrganization`
Extracts the organization claim (`"organization"`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose organization is to be retrieved.
- **Return value**: `string?` – the value of the organization claim, or `null` if the principal is unauthenticated or the claim is missing.
- **Exceptions**: None.

### `GetDepartment`
Extracts the department claim (`"department"`) from the principal.

- **Parameters**: `ClaimsPrincipal principal` – the user whose department is to be retrieved.
- **Return value**: `string?` – the value of the department claim, or `null` if the principal is unauthenticated or the claim is missing.
- **Exceptions**: None.

### `IsAuthenticated`
Checks whether the principal is authenticated.

- **Parameters**: `ClaimsPrincipal principal` – the user to check.
- **Return value**: `bool` – `true` if the principal is authenticated; otherwise, `false`.
- **Exceptions**: None.

## Usage
