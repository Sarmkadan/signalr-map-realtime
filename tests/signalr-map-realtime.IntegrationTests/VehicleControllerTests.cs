// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using SignalRMapRealtime.Models;
using System.Collections.Generic;
using SignalRMapRealtime.DTOs;
using Newtonsoft.Json;
using System.Text;
using System;
using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

namespace SignalRMapRealtime.IntegrationTests
{
    public class VehicleControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public VehicleControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task SeedVehicle(Vehicle vehicle)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Vehicles.Add(vehicle);
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetVehicles_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/api/Vehicle");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PaginatedResponse<VehicleDto>>(responseString);

            pagedResponse.Should().NotBeNull();
            pagedResponse.Items.Should().BeEmpty(); // Assuming no seed data initially
        }

        [Fact]
        public async Task PostVehicle_ReturnsCreatedVehicle()
        {
            // Arrange
            var newVehicle = new VehicleDto
            {
                Make = "Ford",
                Model = "Transit",
                Year = 2020,
                LicensePlate = "TEST123",
                Status = VehicleStatus.Available
            };
            var content = new StringContent(JsonConvert.SerializeObject(newVehicle), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Vehicle", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var createdVehicle = JsonConvert.DeserializeObject<VehicleDto>(responseString);

            createdVehicle.Should().NotBeNull();
            createdVehicle.Id.Should().NotBeEmpty();
            createdVehicle.Make.Should().Be(newVehicle.Make);
            createdVehicle.Model.Should().Be(newVehicle.Model);
        }

        [Fact]
        public async Task GetVehicleById_ReturnsNotFound_ForNonExistentId()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Vehicle/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetVehicleById_ReturnsVehicle_ForExistingId()
        {
            // Arrange
            var existingVehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Make = "Mercedes",
                Model = "Sprinter",
                Year = 2021,
                LicensePlate = "EXIST456",
                Status = VehicleStatus.InUse
            };
            await SeedVehicle(existingVehicle);

            // Act
            var response = await _client.GetAsync($"/api/Vehicle/{existingVehicle.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            var vehicleDto = JsonConvert.DeserializeObject<VehicleDto>(responseString);

            vehicleDto.Should().NotBeNull();
            vehicleDto.Id.Should().Be(existingVehicle.Id);
            vehicleDto.Make.Should().Be(existingVehicle.Make);
            vehicleDto.Model.Should().Be(existingVehicle.Model);
        }

        [Fact]
        public async Task PutVehicle_UpdatesExistingVehicle()
        {
            // Arrange
            var existingVehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Make = "BMW",
                Model = "X5",
                Year = 2019,
                LicensePlate = "UPDATE789",
                Status = VehicleStatus.Available
            };
            await SeedVehicle(existingVehicle);

            var updatedVehicle = new VehicleDto
            {
                Id = existingVehicle.Id,
                Make = "BMW",
                Model = "X5",
                Year = 2022,
                LicensePlate = "UPDATE789",
                Status = VehicleStatus.Maintenance
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedVehicle), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Vehicle/{existingVehicle.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/Vehicle/{existingVehicle.Id}");
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var vehicleDto = JsonConvert.DeserializeObject<VehicleDto>(responseString);

            vehicleDto.Should().NotBeNull();
            vehicleDto.Year.Should().Be(updatedVehicle.Year);
            vehicleDto.Status.Should().Be(updatedVehicle.Status);
        }

        [Fact]
        public async Task DeleteVehicle_RemovesVehicle()
        {
            // Arrange
            var vehicleToDelete = new Vehicle
            {
                Id = Guid.NewGuid(),
                Make = "Audi",
                Model = "Q7",
                Year = 2018,
                LicensePlate = "DELETE000",
                Status = VehicleStatus.Available
            };
            await SeedVehicle(vehicleToDelete);

            // Act
            var response = await _client.DeleteAsync($"/api/Vehicle/{vehicleToDelete.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/Vehicle/{vehicleToDelete.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostVehicle_ReturnsBadRequest_ForInvalidModel()
        {
            // Arrange
            var invalidVehicle = new VehicleDto
            {
                // Missing required fields like Make and Model
                Year = 2023,
                LicensePlate = "BADREQ1",
                Status = VehicleStatus.Available
            };
            var content = new StringContent(JsonConvert.SerializeObject(invalidVehicle), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Vehicle", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutVehicle_ReturnsBadRequest_ForMismatchedId()
        {
            // Arrange
            var existingVehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Make = "VW",
                Model = "Golf",
                Year = 2017,
                LicensePlate = "MATCHID1",
                Status = VehicleStatus.Available
            };
            await SeedVehicle(existingVehicle);

            var updatedVehicle = new VehicleDto
            {
                Id = Guid.NewGuid(), // Mismatched ID
                Make = "VW",
                Model = "Golf",
                Year = 2020,
                LicensePlate = "MATCHID1",
                Status = VehicleStatus.InUse
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedVehicle), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Vehicle/{existingVehicle.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
