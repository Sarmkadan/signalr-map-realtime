namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="User"/> class.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Checks if a user is eligible for vehicle assignment based on their job title and department.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns><c>true</c> if the user is eligible; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static bool IsEligibleForVehicleAssignment(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Users with Driver or Manager job titles in Transportation or Logistics departments are eligible
        var eligibleJobTitles = new[] { "Driver", "Manager" };
        var eligibleDepartments = new[] { "Transportation", "Logistics" };

        return eligibleJobTitles.Contains(user.JobTitle, StringComparer.OrdinalIgnoreCase)
            && eligibleDepartments.Contains(user.Department, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the user's full details, including their last location and assigned vehicles.
    /// </summary>
    /// <param name="user">The user to get details for.</param>
    /// <returns>A string containing the user's full details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static string GetFullDetails(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var lastLocation = user.LastLocation != null
            ? $"Last location: {user.LastLocation.Latitude:F6}, {user.LastLocation.Longitude:F6}"
            : "No last location";

        var assignedVehicles = user.AssignedVehicles.Any()
            ? $"Assigned vehicles: {string.Join(", ", user.AssignedVehicles.Select(v => v.Name))}"
            : "No assigned vehicles";

        return $"{user.FullName} - {user.Email} - {lastLocation} - {assignedVehicles}";
    }

    /// <summary>
    /// Checks if a user has a valid phone number and profile image URL.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns><c>true</c> if the user has a valid phone number and profile image URL; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static bool HasValidContactInfo(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return !string.IsNullOrEmpty(user.PhoneNumber) && !string.IsNullOrEmpty(user.ProfileImageUrl);
    }
}