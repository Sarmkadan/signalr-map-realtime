using System;
using System.Collections.Generic;
using System.Globalization;

namespace SignalRMapRealtime.Services
{
    /// <summary>
    /// Provides validation helpers for tracking session parameters used by <see cref="TrackingService"/>.
    /// </summary>
    public static class TrackingServiceValidation
    {
        private static readonly IReadOnlyList<string> EmptyErrors = Array.Empty<string>();

        /// <summary>
        /// Validates a vehicle ID parameter.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateVehicleId(int vehicleId)
        {
            var errors = new List<string>();

            if (vehicleId <= 0)
            {
                errors.Add($"Vehicle ID must be a positive integer, but was {vehicleId}.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a session ID parameter.
        /// </summary>
        /// <param name="sessionId">The session identifier to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateSessionId(int sessionId)
        {
            var errors = new List<string>();

            if (sessionId <= 0)
            {
                errors.Add($"Session ID must be a positive integer, but was {sessionId}.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a session name parameter.
        /// </summary>
        /// <param name="sessionName">The session name to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateSessionName(string? sessionName)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(sessionName) && sessionName.Length > 200)
            {
                errors.Add("Session name cannot exceed 200 characters.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a route ID parameter.
        /// </summary>
        /// <param name="routeId">The route identifier to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateRouteId(int? routeId)
        {
            var errors = new List<string>();

            if (routeId.HasValue && routeId.Value <= 0)
            {
                errors.Add($"Route ID must be a positive integer or null, but was {routeId}.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a cancellation reason parameter.
        /// </summary>
        /// <param name="reason">The cancellation reason to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateCancellationReason(string? reason)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(reason) && reason.Length > 500)
            {
                errors.Add("Cancellation reason cannot exceed 500 characters.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.StartTrackingSessionAsync"/>.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="sessionName">The optional session name.</param>
        /// <param name="routeId">The optional route identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateStartSessionParameters(
            int vehicleId,
            string? sessionName = null,
            int? routeId = null)
        {
            var errors = new List<string>();

            errors.AddRange(ValidateVehicleId(vehicleId));
            errors.AddRange(ValidateSessionName(sessionName));
            errors.AddRange(ValidateRouteId(routeId));

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.PauseSessionAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidatePauseSessionParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.ResumeSessionAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateResumeSessionParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.CompleteSessionAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateCompleteSessionParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.CancelSessionAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="reason">The optional cancellation reason.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateCancelSessionParameters(
            int sessionId,
            string? reason = null)
        {
            var errors = new List<string>();

            errors.AddRange(ValidateSessionId(sessionId));
            errors.AddRange(ValidateCancellationReason(reason));

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.IsSessionActiveAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateSessionStatusParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.GetSessionDistanceAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateSessionDistanceParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Validates all parameters for <see cref="TrackingService.GetSessionAverageSpeedAsync"/>.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateSessionSpeedParameters(int sessionId)
        {
            return ValidateSessionId(sessionId);
        }

        /// <summary>
        /// Determines whether the specified vehicle ID is valid.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidVehicleId(int vehicleId)
        {
            return ValidateVehicleId(vehicleId).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified session ID is valid.
        /// </summary>
        /// <param name="sessionId">The session identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidSessionId(int sessionId)
        {
            return ValidateSessionId(sessionId).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified session name is valid.
        /// </summary>
        /// <param name="sessionName">The session name to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidSessionName(string? sessionName)
        {
            return ValidateSessionName(sessionName).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified route ID is valid.
        /// </summary>
        /// <param name="routeId">The route identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidRouteId(int? routeId)
        {
            return ValidateRouteId(routeId).Count == 0;
        }

        /// <summary>
        /// Determines whether the specified cancellation reason is valid.
        /// </summary>
        /// <param name="reason">The cancellation reason to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidCancellationReason(string? reason)
        {
            return ValidateCancellationReason(reason).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified vehicle ID is valid.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the vehicle ID is not valid.</exception>
        public static void EnsureValidVehicleId(int vehicleId)
        {
            var errors = ValidateVehicleId(vehicleId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Vehicle ID {vehicleId} is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the specified session ID is valid.
        /// </summary>
        /// <param name="sessionId">The session identifier to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the session ID is not valid.</exception>
        public static void EnsureValidSessionId(int sessionId)
        {
            var errors = ValidateSessionId(sessionId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Session ID {sessionId} is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the specified session name is valid.
        /// </summary>
        /// <param name="sessionName">The session name to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the session name is not valid.</exception>
        public static void EnsureValidSessionName(string? sessionName)
        {
            var errors = ValidateSessionName(sessionName);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Session name is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the specified route ID is valid.
        /// </summary>
        /// <param name="routeId">The route identifier to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the route ID is not valid.</exception>
        public static void EnsureValidRouteId(int? routeId)
        {
            var errors = ValidateRouteId(routeId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Route ID is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the specified cancellation reason is valid.
        /// </summary>
        /// <param name="reason">The cancellation reason to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the cancellation reason is not valid.</exception>
        public static void EnsureValidCancellationReason(string? reason)
        {
            var errors = ValidateCancellationReason(reason);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Cancellation reason is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }
    }
}