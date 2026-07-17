#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Provides validation helpers for location-related DTOs used in <see cref="LocationControllerTests"/>.
    /// </summary>
    public static class LocationControllerTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="DTOs.LocationDto"/> instance.
        /// </summary>
        /// <param name="dto">The location DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DTOs.LocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (dto.Latitude is < -90 or > 90)
            {
                problems.Add("Latitude must be between -90 and 90 degrees.");
            }

            if (dto.Longitude is < -180 or > 180)
            {
                problems.Add("Longitude must be between -180 and 180 degrees.");
            }

            if (dto.Accuracy < 0)
            {
                problems.Add("Accuracy cannot be negative.");
            }

            if (dto.Speed < 0)
            {
                problems.Add("Speed cannot be negative.");
            }

            if (dto.Timestamp > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("Timestamp cannot be in the future.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified <see cref="DTOs.CreateLocationDto"/> instance.
        /// </summary>
        /// <param name="dto">The create location DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DTOs.CreateLocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (dto.Latitude is < -90 or > 90)
            {
                problems.Add("Latitude must be between -90 and 90 degrees.");
            }

            if (dto.Longitude is < -180 or > 180)
            {
                problems.Add("Longitude must be between -180 and 180 degrees.");
            }

            if (dto.Accuracy < 0)
            {
                problems.Add("Accuracy cannot be negative.");
            }

            if (dto.Speed < 0)
            {
                problems.Add("Speed cannot be negative.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified <see cref="DTOs.UpdateLocationDto"/> instance.
        /// </summary>
        /// <param name="dto">The update location DTO to validate.</param>
        /// <returns>A list of validation problems; empty if the DTO is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DTOs.UpdateLocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = new List<string>();

            if (dto.Speed < 0)
            {
                problems.Add("Speed cannot be negative.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DTOs.LocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The location DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this DTOs.LocationDto dto) => Validate(dto).Count == 0;

        /// <summary>
        /// Determines whether the specified <see cref="DTOs.CreateLocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The create location DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this DTOs.CreateLocationDto dto) => Validate(dto).Count == 0;

        /// <summary>
        /// Determines whether the specified <see cref="DTOs.UpdateLocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The update location DTO to check.</param>
        /// <returns><see langword="true"/> if the DTO is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        public static bool IsValid(this DTOs.UpdateLocationDto dto) => Validate(dto).Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="DTOs.LocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The location DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this DTOs.LocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"LocationDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="DTOs.CreateLocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The create location DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this DTOs.CreateLocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"CreateLocationDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="DTOs.UpdateLocationDto"/> instance is valid.
        /// </summary>
        /// <param name="dto">The update location DTO to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the DTO is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this DTOs.UpdateLocationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var problems = Validate(dto);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"UpdateLocationDto instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(dto));
            }
        }
    }
}
