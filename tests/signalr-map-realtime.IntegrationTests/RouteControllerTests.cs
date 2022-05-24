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
using System.Collections.Generic;
using SignalRMapRealtime.DTOs;
using Newtonsoft.Json;
using System.Text;
using System;
using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Tests for the RouteController.
    /// </summary>
    public class RouteControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteControllerTests"/> class.
        /// </summary>
        /// <param name="factory">The test application factory.</param>
        public RouteControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Seeds a route in the database.
        /// </summary>
        /// <param name="route">The route to seed.</param>
        private async Task SeedRoute(Route route)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Routes.Add(route);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Tests that getting all routes returns a successful response with the correct content type.
        /// </summary>
        [Fact]
        public async Task GetRoutes_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/api/Route");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PaginatedResponse<RouteDto>>(responseString);

            pagedResponse.Should().NotBeNull();
            pagedResponse.Items.Should().BeEmpty(); // Assuming no seed data initially
        }

        /// <summary>
        /// Tests that posting a new route returns a created route.
        /// </summary>
        [Fact]
        public async Task PostRoute_ReturnsCreatedRoute()
        {
            // Arrange
            var newRoute = new RouteDto
            {
                Name = "Test Route",
                Description = "A test route description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(newRoute), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Route", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var createdRoute = JsonConvert.DeserializeObject<RouteDto>(responseString);

            createdRoute.Should().NotBeNull();
            createdRoute.Id.Should().BeGreaterThan(0);
            createdRoute.Name.Should().Be(newRoute.Name);
            createdRoute.Description.Should().Be(newRoute.Description);
        }

        /// <summary>
        /// Tests that getting a route by non-existent ID returns a not found response.
        /// </summary>
        [Fact]
        public async Task GetRouteById_ReturnsNotFound_ForNonExistentId()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Route/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Tests that getting a route by existing ID returns the route.
        /// </summary>
        [Fact]
        public async Task GetRouteById_ReturnsRoute_ForExistingId()
        {
            // Arrange
            var existingRoute = new Route
            {
                Name = "Existing Route",
                Description = "Description for existing route"
            };
            await SeedRoute(existingRoute);

            // Act
            var response = await _client.GetAsync($"/api/Route/{existingRoute.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            var routeDto = JsonConvert.DeserializeObject<RouteDto>(responseString);

            routeDto.Should().NotBeNull();
            routeDto.Id.Should().Be(existingRoute.Id);
            routeDto.Name.Should().Be(existingRoute.Name);
        }

        /// <summary>
        /// Tests that updating an existing route returns a no content response.
        /// </summary>
        [Fact]
        public async Task PutRoute_UpdatesExistingRoute()
        {
            // Arrange
            var existingRoute = new Route
            {
                Name = "Route to Update",
                Description = "Original description"
            };
            await SeedRoute(existingRoute);

            var updatedRoute = new RouteDto
            {
                Id = existingRoute.Id,
                Name = "Updated Route Name",
                Description = "Updated description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedRoute), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Route/{existingRoute.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/Route/{existingRoute.Id}");
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var routeDto = JsonConvert.DeserializeObject<RouteDto>(responseString);

            routeDto.Should().NotBeNull();
            routeDto.Name.Should().Be(updatedRoute.Name);
            routeDto.Description.Should().Be(updatedRoute.Description);
        }

        /// <summary>
        /// Tests that deleting a route returns a no content response.
        /// </summary>
        [Fact]
        public async Task DeleteRoute_RemovesRoute()
        {
            // Arrange
            var routeToDelete = new Route
            {
                Name = "Route to Delete",
                Description = "Description to be deleted"
            };
            await SeedRoute(routeToDelete);

            // Act
            var response = await _client.DeleteAsync($"/api/Route/{routeToDelete.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/Route/{routeToDelete.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Tests that posting a new route with invalid model returns a bad request response.
        /// </summary>
        [Fact]
        public async Task PostRoute_ReturnsBadRequest_ForInvalidModel()
        {
            // Arrange
            var invalidRoute = new RouteDto
            {
                // Missing required fields like Name
                Description = "Invalid route description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(invalidRoute), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Route", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Tests that updating a route with mismatched ID returns a bad request response.
        /// </summary>
        [Fact]
        public async Task PutRoute_ReturnsBadRequest_ForMismatchedId()
        {
            // Arrange
            var existingRoute = new Route
            {
                Name = "Route to Update",
                Description = "Original description"
            };
            await SeedRoute(existingRoute);

            var updatedRoute = new RouteDto
            {
                Id = existingRoute.Id + 1, // Mismatched ID
                Name = "Updated Route Name",
                Description = "Updated description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedRoute), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Route/{existingRoute.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
