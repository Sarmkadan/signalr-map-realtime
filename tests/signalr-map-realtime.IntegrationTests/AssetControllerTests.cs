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
    /// <summary>
    /// Tests for the AssetController.
    /// </summary>
    public class AssetControllerTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetControllerTests"/> class.
        /// </summary>
        /// <param name="factory">The test application factory.</param>
        public AssetControllerTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Seeds an asset in the database.
        /// </summary>
        /// <param name="asset">The asset to seed.</param>
        private async Task SeedAsset(Asset asset)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Assets.Add(asset);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Tests that getting assets returns a successful response with the correct content type.
        /// </summary>
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

        /// <summary>
        /// Tests that posting an asset returns a created asset.
        /// </summary>
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

        /// <summary>
        /// Tests that getting an asset by ID returns a not found response for a non-existent ID.
        /// </summary>
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

        /// <summary>
        /// Tests that getting an asset by ID returns the asset for an existing ID.
        /// </summary>
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

        /// <summary>
        /// Tests that updating an asset updates the existing asset.
        /// </summary>
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

        /// <summary>
        /// Tests that deleting an asset removes the asset.
        /// </summary>
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

        /// <summary>
        /// Tests that posting an asset returns a bad request response for an invalid model.
        /// </summary>
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

        /// <summary>
        /// Tests that updating an asset returns a bad request response for a mismatched ID.
        /// </summary>
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
