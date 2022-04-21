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
using SignalRMapRealtime.Domain.Enums;

namespace SignalRMapRealtime.IntegrationTests
{
    public class AssetControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public AssetControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task SeedAsset(Asset asset)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Assets.Add(asset);
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetAssets_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/api/Asset");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PaginatedResponse<AssetDto>>(responseString);

            pagedResponse.Should().NotBeNull();
            pagedResponse.Items.Should().BeEmpty(); // Assuming no seed data initially
        }

        [Fact]
        public async Task PostAsset_ReturnsCreatedAsset()
        {
            // Arrange
            var newAsset = new AssetDto
            {
                Name = "Test Asset",
                Type = AssetType.Equipment,
                Status = "Active"
            };
            var content = new StringContent(JsonConvert.SerializeObject(newAsset), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Asset", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Content.Headers.ContentType.ToString().Should().Contain("application/json");

            var responseString = await response.Content.ReadAsStringAsync();
            var createdAsset = JsonConvert.DeserializeObject<AssetDto>(responseString);

            createdAsset.Should().NotBeNull();
            createdAsset.Id.Should().BeGreaterThan(0);
            createdAsset.Name.Should().Be(newAsset.Name);
            createdAsset.Type.Should().Be(newAsset.Type);
        }

        [Fact]
        public async Task GetAssetById_ReturnsNotFound_ForNonExistentId()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Asset/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAssetById_ReturnsAsset_ForExistingId()
        {
            // Arrange
            var existingAsset = new Asset
            {
                Name = "Existing Asset",
                Type = AssetType.Vehicle,
                Status = "Idle"
            };
            await SeedAsset(existingAsset);

            // Act
            var response = await _client.GetAsync($"/api/Asset/{existingAsset.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            var assetDto = JsonConvert.DeserializeObject<AssetDto>(responseString);

            assetDto.Should().NotBeNull();
            assetDto.Id.Should().Be(existingAsset.Id);
            assetDto.Name.Should().Be(existingAsset.Name);
        }

        [Fact]
        public async Task PutAsset_UpdatesExistingAsset()
        {
            // Arrange
            var existingAsset = new Asset
            {
                Name = "Asset to Update",
                Type = AssetType.Equipment,
                Status = "Active"
            };
            await SeedAsset(existingAsset);

            var updatedAsset = new AssetDto
            {
                Id = existingAsset.Id,
                Name = "Updated Asset Name",
                Type = AssetType.Vehicle,
                Status = "Inactive"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedAsset), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Asset/{existingAsset.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/Asset/{existingAsset.Id}");
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var assetDto = JsonConvert.DeserializeObject<AssetDto>(responseString);

            assetDto.Should().NotBeNull();
            assetDto.Name.Should().Be(updatedAsset.Name);
            assetDto.Type.Should().Be(updatedAsset.Type);
        }

        [Fact]
        public async Task DeleteAsset_RemovesAsset()
        {
            // Arrange
            var assetToDelete = new Asset
            {
                Name = "Asset to Delete",
                Type = AssetType.Equipment,
                Status = "Active"
            };
            await SeedAsset(assetToDelete);

            // Act
            var response = await _client.DeleteAsync($"/api/Asset/{assetToDelete.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/Asset/{assetToDelete.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostAsset_ReturnsBadRequest_ForInvalidModel()
        {
            // Arrange
            var invalidAsset = new AssetDto
            {
                // Missing required fields like Name
                Type = AssetType.Equipment,
                Status = "Active"
            };
            var content = new StringContent(JsonConvert.SerializeObject(invalidAsset), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Asset", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutAsset_ReturnsBadRequest_ForMismatchedId()
        {
            // Arrange
            var existingAsset = new Asset
            {
                Name = "Asset to Update",
                Type = AssetType.Equipment,
                Status = "Active"
            };
            await SeedAsset(existingAsset);

            var updatedAsset = new AssetDto
            {
                Id = existingAsset.Id + 1, // Mismatched ID
                Name = "Updated Asset Name",
                Type = AssetType.Vehicle,
                Status = "Inactive"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updatedAsset), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/Asset/{existingAsset.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
