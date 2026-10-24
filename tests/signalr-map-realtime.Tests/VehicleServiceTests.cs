#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using Xunit;

/// <summary>
/// Tests for the VehicleService class.
/// Repositories are concrete with non-virtual members, so only paths that fail before any data access are covered here.
/// </summary>
public class VehicleServiceTests
{
    /// <summary>
    /// Creates a VehicleService with real repositories over an unused SQL Server context and a real AutoMapper configuration.
    /// </summary>
    private static VehicleService CreateService()
    {
        return new VehicleService(
            CreateVehicleRepository(),
            CreateUserRepository(),
            CreateMapper(),
            Substitute.For<ILogger<VehicleService>>());
    }

    private static VehicleRepository CreateVehicleRepository() => new(CreateContext());

    private static UserRepository CreateUserRepository() => new(CreateContext());

    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()).CreateMapper();

    private static ApplicationDbContext CreateContext()
    {
        // The context is never opened, so no server is contacted.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Constructor_NullDependency_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = CreateVehicleRepository();
        var users = CreateUserRepository();
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<VehicleService>>();

        // Act
        Action noVehicles = () => new VehicleService(null!, users, mapper, logger);
        Action noUsers = () => new VehicleService(repository, null!, mapper, logger);
        Action noMapper = () => new VehicleService(repository, users, null!, logger);
        Action noLogger = () => new VehicleService(repository, users, mapper, null!);

        // Assert
        noVehicles.Should().Throw<ArgumentNullException>().WithParameterName("vehicleRepository");
        noUsers.Should().Throw<ArgumentNullException>().WithParameterName("userRepository");
        noMapper.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        noLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateVehicleAsync_BlankName_ThrowsArgumentException(string name)
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateVehicleDto { Name = name, RegistrationNumber = "AB-1234" };

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateVehicleAsync(dto));

        // Assert
        ex.Message.Should().Be("Vehicle name is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateVehicleAsync_BlankRegistrationNumber_ThrowsArgumentException(string registration)
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateVehicleDto { Name = "Truck 1", RegistrationNumber = registration };

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateVehicleAsync(dto));

        // Assert
        ex.Message.Should().Be("Registration number is required.");
    }

    [Fact]
    public async Task UpdateVehicleAsync_NullDto_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateVehicleAsync(1, null!));
    }
}
