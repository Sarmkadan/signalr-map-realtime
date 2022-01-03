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
using SignalRMapRealtime.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.IntegrationTests
{
    public class LocationControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public LocationControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task SeedLocation(Location location)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetLocations_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            // No seed data needed for this test, assuming an empty database initially.

            // Act
            var response = await _client.GetAsync("/api/Location");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PaginatedResponse<LocationDto>>(responseString);

            pagedResponse.Should().NotBeNull();
            pagedResponse.Items.Should().BeEmpty(); // Assuming no seed data initially
        }

        [Fact]
        public async Task PostLocation_ReturnsCreatedLocation()
        {
            // Arrange
            var newLocation = new LocationDto
            {
                Latitude = 34.0522,
                Longitude = -118.2437,
                Timestamp = System.DateTime.UtcNow,
                Accuracy = 10,
                Speed = 50,
                Heading = 90
            };
            var content = new StringContent(JsonConvert.SerializeObject(newLocation), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Location", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var createdLocation = JsonConvert.DeserializeObject<LocationDto>(responseString);

            createdLocation.Should().NotBeNull();
            createdLocation.Id.Should().NotBeEmpty();
            createdLocation.Latitude.Should().Be(newLocation.Latitude);
            createdLocation.Longitude.Should().Be(newLocation.Longitude);
        }

        [Fact]
        public async Task GetLocationById_ReturnsNotFound_ForNonExistentId()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Location/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetLocationById_ReturnsLocation_ForExistingId()
        {
            // Arrange
            var existingLocation = new Location
            {
                Id = Guid.NewGuid(),
                Latitude = 34.0,
                Longitude = -118.0,
                Timestamp = DateTime.UtcNow,
                Accuracy = 5,
                Speed = 60,
                Heading = 180
            };
            await SeedLocation(existingLocation);

            // Act
            var response = await _client.GetAsync($"/api/Location/{existingLocation.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            var locationDto = JsonConvert.DeserializeObject<LocationDto>(responseString);

            locationDto.Should().NotBeNull();
            locationDto.Id.Should().Be(existingLocation.Id);
            locationDto.Latitude.Should().Be(existingLocation.Latitude);
            locationDto.Longitude.Should().Be(existingLocation.Longitude);
        }

        [Fact]
        public async Task PutLocation_UpdatesExistingLocation()
        {
            // Arrange
            var existingLocation = new Location
            {
                Id = Guid.NewGuid(),
                Latitude = 34.0,
                Longitude = -118.0,
                Timestamp = DateTime.UtcNow,
                Accuracy = 5,
                Speed = 60,
                Heading = 180
            };
            await SeedLocation(existingLocation);

            var updatedLocation = new LocationDto
            {
                Id = existingLocation.Id,
                Latitude = 34.1,
                Longitude = -118.1,
                Timestamp = DateTime.UtcNow.AddMinutes(1),
                Accuracy = 6,
                Speed = 65,
                Heading = 190
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedLocation), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Location/{existingLocation.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/Location/{existingLocation.Id}");
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var locationDto = JsonConvert.DeserializeObject<LocationDto>(responseString);

            locationDto.Should().NotBeNull();
            locationDto.Latitude.Should().Be(updatedLocation.Latitude);
            locationDto.Longitude.Should().Be(updatedLocation.Longitude);
            locationDto.Accuracy.Should().Be(updatedLocation.Accuracy);
        }

        [Fact]
        public async Task DeleteLocation_RemovesLocation()
        {
            // Arrange
            var locationToDelete = new Location
            {
                Id = Guid.NewGuid(),
                Latitude = 34.0,
                Longitude = -118.0,
                Timestamp = DateTime.UtcNow,
                Accuracy = 5,
                Speed = 60,
                Heading = 180
            };
            await SeedLocation(locationToDelete);

            // Act
            var response = await _client.DeleteAsync($"/api/Location/{locationToDelete.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/Location/{locationToDelete.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostLocation_ReturnsBadRequest_ForInvalidModel()
        {
            // Arrange
            var invalidLocation = new LocationDto
            {
                // Missing required fields like Latitude and Longitude
                Timestamp = System.DateTime.UtcNow
            };
            var content = new StringContent(JsonConvert.SerializeObject(invalidLocation), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Location", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutLocation_ReturnsBadRequest_ForMismatchedId()
        {
            // Arrange
            var existingLocation = new Location
            {
                Id = Guid.NewGuid(),
                Latitude = 34.0,
                Longitude = -118.0,
                Timestamp = DateTime.UtcNow,
                Accuracy = 5,
                Speed = 60,
                Heading = 180
            };
            await SeedLocation(existingLocation);

            var updatedLocation = new LocationDto
            {
                Id = Guid.NewGuid(), // Mismatched ID
                Latitude = 34.1,
                Longitude = -118.1,
                Timestamp = DateTime.UtcNow.AddMinutes(1)
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedLocation), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Location/{existingLocation.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
