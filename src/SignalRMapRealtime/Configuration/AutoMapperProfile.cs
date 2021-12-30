// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

using AutoMapper;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;

/// <summary>
/// AutoMapper profiles for mapping between domain models and DTOs.
/// </summary>
public class AutoMapperProfile : Profile
{
    /// <summary>
    /// Initializes a new instance and configures all mappings.
    /// </summary>
    public AutoMapperProfile()
    {
        // Location mappings
        CreateMap<Location, LocationDto>().ReverseMap();
        CreateMap<CreateLocationDto, Location>();
        CreateMap<UpdateLocationDto, Location>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Vehicle mappings
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(dest => dest.LastLocation, opt => opt.MapFrom(src => src.LastLocation));
        CreateMap<CreateVehicleDto, Vehicle>();
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // User mappings
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Asset mappings
        CreateMap<Asset, AssetDto>().ReverseMap();
        CreateMap<CreateAssetDto, Asset>();
        CreateMap<UpdateAssetDto, Asset>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Route mappings
        CreateMap<Route, RouteDto>()
            .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
            .ForMember(dest => dest.AssignedUser, opt => opt.MapFrom(src => src.AssignedUser))
            .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => src.Waypoints));
        CreateMap<CreateRouteDto, Route>();
        CreateMap<UpdateRouteDto, Route>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Waypoint mappings
        CreateMap<Waypoint, WaypointDto>().ReverseMap();
        CreateMap<CreateWaypointDto, Waypoint>();
        CreateMap<UpdateWaypointDto, Waypoint>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
