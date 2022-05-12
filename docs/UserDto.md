# UserDto

The `UserDto` class serves as a lightweight data transfer object for user information within the `signalr-map-realtime` project. It encapsulates identity, contact, organizational, and presence data for transmission across application layers, typically between the server and SignalR-connected clients. The type is designed for serialization and carries no business logic.

## API

### public int Id
Gets or sets the unique integer identifier for the user. This value corresponds to the primary key in the underlying data store.

### public string FullName
Gets or sets the user's full display name. This property is required and should not be null or empty when representing a valid user.

### public string Email
Gets or sets the user's primary email address. This property is required and serves as a unique contact identifier.

### public string? PhoneNumber
Gets or sets the user's phone number. This value is nullable; a null indicates the phone number has not been provided or is not applicable.

### public string? EmployeeId
Gets or sets an optional employee identifier, such as a payroll or HR system code. Null when the user is not associated with an employee record or the value is unavailable.

### public string? JobTitle
Gets or sets the user's job title or position. Nullable when the title is not set or not relevant.

### public string? Department
Gets or sets the department or organizational unit the user belongs to. Nullable when the department assignment is absent.

### public bool IsOnline
Gets or sets a value indicating whether the user is currently connected or considered online. This is typically updated via SignalR presence events.

### public bool IsActive
Gets or sets a value indicating whether the user account is active. Inactive users may be prevented from authenticating or appearing in active user lists.

### public DateTime? LastLoginAt
Gets or sets the timestamp of the user's most recent login. Null if the user has never logged in.

### public DateTime CreatedAt
Gets or sets the timestamp when the user record was created. This value is expected to be set upon initial persistence and is not nullable.

## Usage

### Example 1: Preparing a DTO for an Online User Broadcast

```csharp
var userDto = new UserDto
{
    Id = 42,
    FullName = "Jane Doe",
    Email = "jane.doe@example.com",
    PhoneNumber = "+1-555-1234",
    EmployeeId = "EMP-001",
    JobTitle = "Software Engineer",
    Department = "Engineering",
    IsOnline = true,
    IsActive = true,
    LastLoginAt = DateTime.UtcNow.AddMinutes(-5),
    CreatedAt = new DateTime(2023, 1, 15, 9, 0, 0, DateTimeKind.Utc)
};

// Serialize and send via SignalR hub context
await hubContext.Clients.All.SendAsync("UserPresenceChanged", userDto);
```

### Example 2: Constructing a Minimal DTO from a Database Entity

```csharp
var userDto = new UserDto
{
    Id = dbUser.Id,
    FullName = dbUser.FullName,
    Email = dbUser.Email,
    IsOnline = false,
    IsActive = dbUser.IsActive,
    CreatedAt = dbUser.CreatedAt
};

// LastLoginAt and optional fields remain null when not available
```

## Notes

- **Nullability**: `PhoneNumber`, `EmployeeId`, `JobTitle`, `Department`, and `LastLoginAt` are explicitly nullable. Consumers must perform null checks before dereferencing these values.
- **Required fields**: `Id`, `FullName`, `Email`, `IsOnline`, `IsActive`, and `CreatedAt` are non-nullable value types or strings expected to hold meaningful data. Assigning null to `FullName` or `Email` may cause downstream serialization or validation failures.
- **Thread safety**: This type is a plain data container with public get/set accessors. It provides no internal synchronization. Concurrent reads and writes from multiple threads must be guarded externally if the same instance is shared.
- **Serialization**: The type is intended for use with JSON serializers (e.g., System.Text.Json or Newtonsoft.Json). Nullable members will be omitted or serialized as `null` depending on serializer configuration.
- **Presence consistency**: `IsOnline` reflects real-time connection state. There is no built-in guarantee that `IsOnline` and `LastLoginAt` remain synchronized; updating both atomically is the responsibility of the calling code.
