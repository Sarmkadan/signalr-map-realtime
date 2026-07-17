#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Provides validation helpers for <see cref="AssetControllerTests"/> instances.
    /// </summary>
    public static class AssetControllerTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="AssetControllerTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AssetControllerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified <see cref="AssetDto"/> instance.
        /// </summary>
        /// <param name="dto">The asset DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                problems.Add("Asset name is required and cannot be empty or whitespace.");
            }

            if (dto.Name?.Length > 100)
            {
                problems.Add("Asset name cannot exceed 100 characters.");
            }

            if (dto.AssetType == default)
            {
                problems.Add("Asset type must be specified.");
            }

            if (dto.Value.HasValue && dto.Value < 0)
            {
                problems.Add("Asset value cannot be negative.");
            }

            if (dto.VehicleId.HasValue && dto.VehicleId < 0)
            {
                problems.Add("Vehicle ID cannot be negative.");
            }

            if (dto.SerialNumber?.Length > 50)
            {
                problems.Add("Serial number cannot exceed 50 characters.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified <see cref="CreateAssetDto"/> instance.
        /// </summary>
        /// <param name="dto">The create asset DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this CreateAssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                problems.Add("Asset name is required and cannot be empty or whitespace.");
            }

            if (dto.Name?.Length > 100)
            {
                problems.Add("Asset name cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
            {
                problems.Add("Serial number is required and cannot be empty or whitespace.");
            }

            if (dto.SerialNumber?.Length > 50)
            {
                problems.Add("Serial number cannot exceed 50 characters.");
            }

            if (dto.AssetType == default)
            {
                problems.Add("Asset type must be specified.");
            }

            if (dto.Value.HasValue && dto.Value < 0)
            {
                problems.Add("Asset value cannot be negative.");
            }

            if (dto.VehicleId.HasValue && dto.VehicleId < 0)
            {
                problems.Add("Vehicle ID cannot be negative.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified <see cref="UpdateAssetDto"/> instance.
        /// </summary>
        /// <param name="dto">The update asset DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this UpdateAssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (dto.Name?.Length > 100)
            {
                problems.Add("Asset name cannot exceed 100 characters.");
            }

            if (dto.Value.HasValue && dto.Value < 0)
            {
                problems.Add("Asset value cannot be negative.");
            }

            if (dto.VehicleId.HasValue && dto.VehicleId < 0)
            {
                problems.Add("Vehicle ID cannot be negative.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="AssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The asset DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this AssetDto dto)
        {
            return Validate(dto).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="CreateAssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The create asset DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this CreateAssetDto dto)
        {
            return Validate(dto).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="UpdateAssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The update asset DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this UpdateAssetDto dto)
        {
            return Validate(dto).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="AssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The asset DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this AssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"AssetDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="CreateAssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The create asset DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this CreateAssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"CreateAssetDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="UpdateAssetDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The update asset DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this UpdateAssetDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"UpdateAssetDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }
    }
}
