#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Domain.Enums;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Extension methods for <see cref="AssetControllerTests"/> to provide reusable test assertions.
    /// </summary>
    public static class AssetControllerTestsExtensions
    {
        /// <summary>
        /// Verifies that the response is a successful OK response with JSON content type.
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task<string> ShouldBeSuccessfulJsonResponse(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var response = await responseTask;
            response.StatusCode.Should().Be(HttpStatusCode.OK, because);
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/json", because);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Verifies that the response is a created response (201) with JSON content type and returns the created resource.
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <typeparam name="T">The type of resource being created.</typeparam>
        /// <returns>The deserialized resource of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task<T> ShouldBeCreatedResource<T>(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var response = await responseTask;
            response.StatusCode.Should().Be(HttpStatusCode.Created, because);
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/json", because);

            var responseString = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<T>(responseString);
            resource.Should().NotBeNull(because);

            return resource;
        }

        /// <summary>
        /// Verifies that the response is a successful OK response and deserializes the response to a paginated list of assets.
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <returns>A read-only list of asset DTOs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task<IReadOnlyList<AssetDto>> ShouldReturnAssetList(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var responseString = await responseTask.ShouldBeSuccessfulJsonResponse(because);
            var pagedResponse = JsonConvert.DeserializeObject<PaginatedResponse<AssetDto>>(responseString);

            pagedResponse.Should().NotBeNull(because);
            pagedResponse.Items.Should().NotBeNull(because);

            return pagedResponse.Items;
        }

        /// <summary>
        /// Verifies that the response is a successful OK response and deserializes the response to a single asset.
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="expectedId">The expected asset ID.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <returns>The deserialized asset DTO.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task<AssetDto> ShouldReturnAsset(this Task<HttpResponseMessage> responseTask, int expectedId, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var responseString = await responseTask.ShouldBeSuccessfulJsonResponse(because);
            var asset = JsonConvert.DeserializeObject<AssetDto>(responseString);

            asset.Should().NotBeNull(because);
            asset.Id.Should().Be(expectedId, because);

            return asset;
        }

        /// <summary>
        /// Verifies that the response is a not found response (404).
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task ShouldBeNotFound(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var response = await responseTask;
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, because);
        }

        /// <summary>
        /// Verifies that the response is a no content response (204) indicating successful deletion.
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task ShouldIndicateSuccessfulDeletion(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var response = await responseTask;
            response.StatusCode.Should().Be(HttpStatusCode.NoContent, because);
        }

        /// <summary>
        /// Verifies that the response is a bad request response (400).
        /// </summary>
        /// <param name="responseTask">The HTTP response to verify.</param>
        /// <param name="because">Optional reason for the assertion.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseTask"/> is null.</exception>
        public static async Task ShouldBeBadRequest(this Task<HttpResponseMessage> responseTask, string because = null)
        {
            ArgumentNullException.ThrowIfNull(responseTask);

            var response = await responseTask;
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);
        }

        /// <summary>
        /// Creates a new asset DTO with default values for testing.
        /// </summary>
        /// <param name="name">The asset name. Defaults to "Test Asset {id}" if null.</param>
        /// <param name="type">The asset type. Defaults to <see cref="AssetType.Equipment"/>.</param>
        /// <param name="status">The asset status. Defaults to "Active".</param>
        /// <returns>A new asset DTO.</returns>
        public static AssetDto CreateTestAssetDto(string name = null, AssetType type = AssetType.Equipment, string status = "Active")
            => new AssetDto
            {
                Name = name ?? $"Test Asset {Guid.NewGuid().ToString()[..8]}",
                Type = type,
                Status = status
            };

        /// <summary>
        /// Creates a new asset DTO with the specified ID for testing.
        /// </summary>
        /// <param name="id">The asset ID (must be greater than 0).</param>
        /// <param name="name">The asset name. Defaults to "Test Asset {id}" if null.</param>
        /// <param name="type">The asset type. Defaults to <see cref="AssetType.Equipment"/>.</param>
        /// <param name="status">The asset status. Defaults to "Active".</param>
        /// <returns>A new asset DTO with the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than 1.</exception>
        public static AssetDto CreateTestAssetDto(int id, string name = null, AssetType type = AssetType.Equipment, string status = "Active")
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Asset ID must be greater than 0");
            }

            return new AssetDto
            {
                Id = id,
                Name = name ?? $"Test Asset {id}",
                Type = type,
                Status = status
            };
        }
    }
}