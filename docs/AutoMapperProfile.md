# AutoMapperProfile

`AutoMapperProfile` is the AutoMapper `Profile` that registers all mappings between domain models (`SignalRMapRealtime.Domain.Models`) and DTOs (`SignalRMapRealtime.DTOs`). It holds no state and has no constructor parameters; its single constructor declares every map.

Source: `src/SignalRMapRealtime/Configuration/AutoMapperProfile.cs`

## Mapping overview

Six entity groups are configured. Each group follows one of two shapes:

| Group | Entity to DTO | DTO to entity (create) | DTO to entity (update) | Reverse map |
|---|---|---|---|---|
| Location | `Location` → `LocationDto` | `CreateLocationDto` → `Location` | `UpdateLocationDto` → `Location` (conditional) | `LocationDto` → `Location` |
| Vehicle | `Vehicle` → `VehicleDto` | `CreateVehicleDto` → `Vehicle` | `UpdateVehicleDto` → `Vehicle` (conditional) | None |
| User | `User` → `UserDto` | `CreateUserDto` → `User` | `UpdateUserDto` → `User` (conditional) | `UserDto` → `User` |
| Asset | `Asset` → `AssetDto` | `CreateAssetDto` → `Asset` | `UpdateAssetDto` → `Asset` (conditional) | `AssetDto` → `Asset` |
| Route | `Route` → `RouteDto` | `CreateRouteDto` → `Route` | `UpdateRouteDto` → `Route` (conditional) | None |
| Waypoint | `Waypoint` → `WaypointDto` | `CreateWaypointDto` → `Waypoint` | `UpdateWaypointDto` → `Waypoint` (conditional) | `WaypointDto` → `Waypoint` |

That is 18 `CreateMap` registrations in total: three per group.

- **Entity → DTO** is the read path, used when returning data from controllers.
- **Create DTO → entity** is the write path for new records. It is one-way and uses default AutoMapper member matching.
- **Update DTO → entity** is the partial-update path. Every member has a condition (see below).
- **Reverse map** is a `ReverseMap()` call on the read mapping, so the same default matching also runs from DTO back to entity. Vehicle and Route do not have one.

## Default rules

Unless a mapping says otherwise, AutoMapper's default convention applies: destination members are matched to source members by name, and no transformation is performed. The file uses no custom value resolvers, type converters, or `Ignore()` calls.

## Explicit member mappings

Three read mappings declare members explicitly with `ForMember(..., opt => opt.MapFrom(src => src.X))`. Each maps a property to a property of the same name, so the result matches the default convention. The explicit form lists which members the DTO receives.

| Mapping | Member | Source expression |
|---|---|---|
| `Vehicle` → `VehicleDto` | `LastLocation` | `src.LastLocation` |
| `Route` → `RouteDto` | `Vehicle` | `src.Vehicle` |
| `Route` → `RouteDto` | `AssignedUser` | `src.AssignedUser` |
| `Route` → `RouteDto` | `Waypoints` | `src.Waypoints` |

## Conditional update mappings

All six `Update*Dto` → entity mappings use the same rule:

```csharp
.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));
```

Each destination member is written only when the corresponding DTO member is not `null`. A `null` value in the update DTO leaves the existing entity value unchanged. This makes the update DTOs partial: a client sends only the fields it wants to change.

Points to keep in mind:

- The condition checks the **source member** only. A `null` in the DTO means "do not change", so a value cannot be set to `null` through these mappings.
- `srcMember` is typed `object?` in this delegate. A non-nullable value-type property (for example `int` or `DateTime`) is boxed before the check and is never `null`. For those properties the condition is always true, so the value is always copied. Only nullable reference types and nullable value types (`int?`, `DateTime?`, and so on) actually skip when `null`.

## Behavior not defined in this file

The file does not show:

- where the profile is registered with dependency injection (for example `AddAutoMapper` in `Program.cs`),
- whether `AssertConfigurationIsValid()` is called,
- how `Id` and other server-managed properties are protected on create or update.

These are determined by code outside this file and are not covered here.

## Related

- `src/SignalRMapRealtime/Domain/Models/` - entities used as mapping sources and destinations.
- `src/SignalRMapRealtime/DTOs/` - DTOs used in the mappings above.
