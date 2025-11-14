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
    public class RouteControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public RouteControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task SeedRoute(Route route)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Routes.Add(route);
                await dbContext.SaveChangesAsync();
            }
        }

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
            createdRoute.Id.Should().NotBeEmpty();
            createdRoute.Name.Should().Be(newRoute.Name);
            createdRoute.Description.Should().Be(newRoute.Description);
        }

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

        [Fact]
        public async Task GetRouteById_ReturnsRoute_ForExistingId()
        {
            // Arrange
            var existingRoute = new Route
            {
                Id = Guid.NewGuid(),
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

        [Fact]
        public async Task PutRoute_UpdatesExistingRoute()
        {
            // Arrange
            var existingRoute = new Route
            {
                Id = Guid.NewGuid(),
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

        [Fact]
        public async Task DeleteRoute_RemovesRoute()
        {
            // Arrange
            var routeToDelete = new Route
            {
                Id = Guid.NewGuid(),
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

        [Fact]
        public async Task PutRoute_ReturnsBadRequest_ForMismatchedId()
        {
            // Arrange
            var existingRoute = new Route
            {
                Id = Guid.NewGuid(),
                Name = "Route to Update",
                Description = "Original description"
            };
            await SeedRoute(existingRoute);

            var updatedRoute = new RouteDto
            {
                Id = Guid.NewGuid(), // Mismatched ID
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
