using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Services;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_Throws_IfServicesNull()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddApplicationServices(null!, configuration));
    }

    [Fact]
    public void AddApplicationServices_Throws_IfConfigurationNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddApplicationServices(services, null!));
    }

    [Fact]
    public void AddApplicationServices_Throws_IfConnectionStringMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DependencyInjection.AddApplicationServices(services, configuration));
    }

    [Fact]
    public void AddApplicationServices_RegistersServices_Success()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Data Source=:memory:" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        DependencyInjection.AddApplicationServices(services, configuration);

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ApplicationDbContext));
        Assert.Contains(services, d => d.ServiceType == typeof(VehicleRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ILocationService));
        Assert.Contains(services, d => d.ServiceType == typeof(IVehicleService));
        Assert.Contains(services, d => d.ServiceType == typeof(ITrackingService));
    }

    [Fact]
    public void AddSignalRServices_Throws_IfServicesNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddSignalRServices(null!));
    }

    [Fact]
    public void AddSignalRServices_RegistersSignalR_Success()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        DependencyInjection.AddSignalRServices(services);

        // Assert
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddSwaggerDocumentation_Throws_IfServicesNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DependencyInjection.AddSwaggerDocumentation(null!));
    }

    [Fact]
    public void AddSwaggerDocumentation_RegistersSwagger_Success()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        DependencyInjection.AddSwaggerDocumentation(services);

        // Assert
        Assert.True(services.Count > 0);
    }
}
