using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SignalRMapRealtime.DTOs
{
    /// <summary>
    /// Provides extension methods for the <see cref="UserDto"/> class to enhance formatting, validation, and data transformation capabilities.
    /// </summary>
    public static class UserDtoExtensions
    {
        /// <summary>
        /// Formats a user's core information into a human-readable summary string.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> to format.</param>
        /// <returns>Formatted summary string containing name, email, and optional phone number.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        public static string FormatUserSummary(this UserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return string.IsNullOrEmpty(user.PhoneNumber)
                ? $"{user.FullName} ({user.Email})"
                : $"{user.FullName} ({user.Email}), {user.PhoneNumber}";
        }

        /// <summary>
        /// Converts a UserDto to a display-ready model with formatted dates.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> to convert.</param>
        /// <returns>Anonymous object with formatted display properties.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null or <see cref="UserDto.CreatedAt"/> is null.</exception>
        public static object ToDisplayModel(this UserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(user.CreatedAt);

            return new
            {
                user.Id,
                user.FullName,
                user.Email,
                PhoneNumber = user.PhoneNumber ?? "Not provided",
                Status = user.IsActive ? "Active" : "Inactive",
                LastLogin = user.LastLoginAt?.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture) ?? "Never logged in",
                CreatedAt = user.CreatedAt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Validates that all required user fields are populated.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> to validate.</param>
        /// <returns>Read-only collection of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        public static IReadOnlyList<string> ValidateRequiredFields(this UserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return (string.IsNullOrEmpty(user.FullName), string.IsNullOrEmpty(user.Email)) switch
            {
                (true, true) => new[] { "Full name is required", "Email is required" },
                (true, false) => new[] { "Full name is required" },
                (false, true) => new[] { "Email is required" },
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// Extracts and formats all available contact information from the user DTO.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> to extract contact info from.</param>
        /// <returns>Read-only collection of contact information strings.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        public static IReadOnlyList<string> GetContactInformation(this UserDto user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return new[]
            {
                user.Email,
                user.PhoneNumber,
                user.EmployeeId
            }
            .Where(contact => !string.IsNullOrEmpty(contact))
            .ToList()
            .AsReadOnly();
        }
    }
}
