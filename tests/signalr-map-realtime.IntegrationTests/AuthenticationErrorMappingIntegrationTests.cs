#nullable enable
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
using SignalRMapRealtime.DTOs;
using Newtonsoft.Json;
using System;
using SignalRMapRealtime.Exceptions;
using SignalRMapRealtime.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Data;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Integration tests for authentication and error mapping via WebApplicationFactory.
    /// Tests cover 401 without API key, 404 body shape for VehicleNotFoundException,
    /// 400 for InvalidLocationException, and pagination edge cases.
    /// </summary>
    public class AuthenticationErrorMappingIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationErrorMappingIntegrationTests"/> class.
        /// </summary>
        /// <param name="factory">The test application factory.</param>
        public AuthenticationErrorMappingIntegrationTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Tests that requests without API key return 401 Unauthorized.
        /// </summary>
        [Fact]
        public async Task Requests_WithoutApiKey_Return401Unauthorized()
        {
            // Arrange - No API key is set in the client

            // Act - Try to access any endpoint without authentication
            var response = await _client.GetAsync("/api/Vehicle");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.ErrorCode.Should().Be("UNAUTHORIZED");
            errorResponse.StatusCode.Should().Be(401);
            errorResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Tests that requests with invalid API key return 401 Unauthorized.
        /// </summary>
        [Fact]
        public async Task Requests_WithInvalidApiKey_Return401Unauthorized()
        {
            // Arrange - Set an invalid API key
            _client.DefaultRequestHeaders.Add("X-API-KEY", "invalid-api-key-123");

            // Act - Try to access endpoint with invalid key
            var response = await _client.GetAsync("/api/Vehicle");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.ErrorCode.Should().Be("UNAUTHORIZED");
            errorResponse.StatusCode.Should().Be(401);
        }

        /// <summary>
        /// Tests that VehicleNotFoundException returns 404 with proper error response body.
        /// </summary>
        [Fact]
        public async Task VehicleNotFoundException_Returns404WithStandardizedErrorBody()
        {
            // Arrange - Seed a vehicle first
            var existingVehicle = new Vehicle
            {
                Make = "Ford",
                Model = "Transit",
                Year = 2020,
                LicensePlate = "TEST123",
                Status = Domain.Enums.VehicleStatus.Available
            };
            await SeedVehicle(existingVehicle);

            // Act - Try to get a different vehicle that doesn't exist
            var nonExistentId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/Vehicle/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.Message.Should().Contain("not found");
            errorResponse.ErrorCode.Should().Be("NOT_FOUND");
            errorResponse.StatusCode.Should().Be(404);
            errorResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Tests that InvalidLocationException returns 400 with proper error response body.
        /// </summary>
        [Fact]
        public async Task InvalidLocationException_Returns400WithStandardizedErrorBody()
        {
            // Arrange - Try to create a vehicle with invalid location coordinates
            var invalidLocationVehicle = new VehicleDto
            {
                Make = "Test",
                Model = "Vehicle",
                Year = 2023,
                LicensePlate = "INVALID1",
                Status = Domain.Enums.VehicleStatus.Available,
                // Invalid coordinates that would trigger InvalidLocationException
                LastLocation = new LocationDto
                {
                    Latitude = 200.0, // Invalid latitude (> 90)
                    Longitude = -300.0, // Invalid longitude (< -180)
                    Timestamp = DateTime.UtcNow,
                    Accuracy = 5.0
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(invalidLocationVehicle), System.Text.Encoding.UTF8, "application/json");

            // Act - Try to create vehicle with invalid location
            var response = await _client.PostAsync("/api/Vehicle", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.Message.Should().Contain("Invalid location");
            errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");
            errorResponse.StatusCode.Should().Be(400);
            errorResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Tests pagination edge case: page=0 should return 400 Bad Request.
        /// </summary>
        [Fact]
        public async Task Pagination_PageZero_Returns400BadRequest()
        {
            // Arrange - No API key needed as this is tested without authentication
            var factoryWithoutAuth = new WebApplicationFactory<Program>();
            var clientWithoutAuth = factoryWithoutAuth.CreateClient();

            // Act - Request with page=0 (invalid pagination parameter)
            var response = await clientWithoutAuth.GetAsync("/api/Vehicle?pageNumber=0&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");
            errorResponse.StatusCode.Should().Be(400);
        }

        /// <summary>
        /// Tests pagination edge case: oversized pageSize should return 400 Bad Request.
        /// </summary>
        [Fact]
        public async Task Pagination_OversizedPageSize_Returns400BadRequest()
        {
            // Arrange - No API key needed as this is tested without authentication
            var factoryWithoutAuth = new WebApplicationFactory<Program>();
            var clientWithoutAuth = factoryWithoutAuth.CreateClient();

            // Act - Request with oversized pageSize (e.g., 10000)
            var response = await clientWithoutAuth.GetAsync("/api/Vehicle?pageNumber=1&pageSize=10000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            errorResponse.Should().NotBeNull();
            errorResponse.Message.Should().NotBeNullOrEmpty();
            errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");
            errorResponse.StatusCode.Should().Be(400);
        }

        /// <summary>
        /// Tests that authenticated requests work correctly with valid API key.
        /// </summary>
        [Fact]
        public async Task AuthenticatedRequests_WithValidApiKey_ReturnSuccess()
        {
            // Arrange - Set valid API key
            _client.DefaultRequestHeaders.Add("X-API-KEY", "test-api-key");

            // Act - Access endpoint with valid authentication
            var response = await _client.GetAsync("/api/Vehicle");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");
        }

        /// <summary>
        /// Helper method to seed a vehicle in the database.
        /// </summary>
        /// <param name="vehicle">The vehicle to seed.</param>
        private async Task SeedVehicle(Vehicle vehicle)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Vehicles.Add(vehicle);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}