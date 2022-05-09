# User

The `User` type represents a person within the real-time mapping system. It holds identity and contact information, an optional profile image, employment metadata, and flags for activity and online presence. It also tracks the user’s last known geographic location and maintains collections of assigned vehicles and routes. The type exposes methods to update online status, validate the email address, refresh the current location, and deactivate the account.

## API

### Properties

- **`public int Id`**  
  Unique numeric identifier for the user. This value is assigned on creation and never changes.

- **`public string FullName`**  
  The user’s full display name. Must not be null.

- **`public string Email`**  
  Primary email address used for login and notifications. Must not be null.

- **`public string? PhoneNumber`**  
  Optional contact phone number. Null when not provided.

- **`public string? EmployeeId`**  
  Optional corporate employee identifier. Null when not assigned.

- **`public string? JobTitle`**  
  Optional job title or role description. Null when not set.

- **`public string? Department`**  
  Optional department or organisational unit name. Null when not set.

- **`public string? ProfileImageUrl`**  
  Optional URL pointing to the user’s profile image. Null when no image is uploaded.

- **`public bool IsActive`**  
  Indicates whether the user account is active. Inactive accounts are prevented from signing in and their resources may be temporarily suspended.

- **`public bool IsOnline`**  
  Indicates whether the user is currently considered online. Updated by `SetOnlineStatus`.

- **`public Location? LastLocation`**  
  The most recent geographic position recorded for the user. Null when no location has ever been captured. The `Location` type is expected to contain latitude, longitude, and an optional timestamp.

- **`public ICollection<Vehicle> AssignedVehicles`**  
  Collection of vehicles currently assigned to the user. May be empty. Modifications to this collection should be performed through the owning context or repository; direct manipulation may not be persisted automatically.

- **`public ICollection<Route> AssignedRoutes`**  
  Collection of routes currently assigned to the user. May be empty. As with `AssignedVehicles`, persistence behaviour depends on the data layer.

- **`public DateTime CreatedAt`**  
  UTC timestamp of when the user record was first created. Immutable after creation.

- **`public DateTime UpdatedAt`**  
  UTC timestamp of the last modification to any user property or related collection. Updated automatically by the persistence layer.

- **`public DateTime? LastLoginAt`**  
  UTC timestamp of the most recent successful login. Null if the user has never logged in.

### Methods

- **`public void SetOnlineStatus(bool isOnline)`**  
  Sets the `IsOnline` flag to the supplied value.  
  *Parameters:* `isOnline` — `true` to mark the user as online; `false` to mark as offline.  
  *Return value:* None.  
  *Exceptions:* Does not throw.

- **`public bool IsEmailValid()`**  
  Validates the `Email` property against a standard email format rule.  
  *Return value:* `true` if the email matches the expected pattern; otherwise `false`.  
  *Exceptions:* Does not throw. Returns `false` for null or empty strings.

- **`public void UpdateLocation(Location newLocation)`**  
  Replaces `LastLocation` with the supplied location value.  
  *Parameters:* `newLocation` — a non-null `Location` instance representing the current position.  
  *Return value:* None.  
  *Exceptions:* Throws `ArgumentNullException` when `newLocation` is null.

- **`public void Deactivate()`**  
  Sets `IsActive` to `false` and `IsOnline` to `false`, effectively disabling the account and marking the user as offline.  
  *Return value:* None.  
  *Exceptions:* Does not throw. Calling this on an already inactive user leaves the state unchanged.

## Usage

### Example 1: Updating location and checking online status

```csharp
// Assume user and location are already retrieved
User currentUser = userRepository.GetById(123);
Location newPosition = locationService.GetCurrentPosition();

// Update the user's last known location
currentUser.UpdateLocation(newPosition);

// Mark the user as online after location refresh
currentUser.SetOnlineStatus(true);

if (currentUser.IsOnline)
{
    Console.WriteLine($"{currentUser.FullName} is now online at {newPosition.Latitude}, {newPosition.Longitude}");
}
```

### Example 2: Validating email and deactivating an account

```csharp
User userToReview = userRepository.GetByEmail("jane.doe@example.com");

if (!userToReview.IsEmailValid())
{
    // Email format is invalid; deactivate the account as a safety measure
    userToReview.Deactivate();
    userRepository.Save(userToReview);
    Console.WriteLine($"User {userToReview.Id} deactivated due to invalid email.");
}
else
{
    Console.WriteLine($"User {userToReview.Id} email is valid. Account remains active.");
}
```

## Notes

- **Thread safety:** The type does not implement its own synchronisation. When multiple threads may read or write properties — particularly `IsOnline`, `LastLocation`, and the `AssignedVehicles`/`AssignedRoutes` collections — external locking or use of thread-safe contexts is required to avoid race conditions.
- **Nullability of `LastLocation`:** `LastLocation` is null until the first successful call to `UpdateLocation`. Code consuming this property must guard against null before accessing coordinates.
- **`Deactivate` idempotency:** Calling `Deactivate` on an already inactive user does not throw and does not change any other state. It will not reset `LastLoginAt` or clear collections.
- **Collection persistence:** `AssignedVehicles` and `AssignedRoutes` are navigation properties. Adding or removing items directly on the collection may not be persisted unless the change tracker of the underlying ORM detects the modification. Prefer modifying assignments through dedicated service or repository methods.
- **Email validation scope:** `IsEmailValid` performs a format check only. It does not verify that the domain exists or that the mailbox can receive messages. Business rules for uniqueness or ownership must be enforced separately.
- **`SetOnlineStatus` and `IsOnline`:** This method does not automatically update `LastLoginAt`. Login timestamps are managed by the authentication flow, not by toggling online presence.
