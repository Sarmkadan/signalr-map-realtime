using System;

namespace SignalRMapRealtime.Models
{
    /// <summary>
    /// Extension methods that make working with <see cref="ApiResponse"/> and <see cref="ApiResponse{T}"/> more convenient.
    /// </summary>
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the response indicates success; otherwise <c>false</c>.
        /// </summary>
        public static bool IsSuccessful(this ApiResponse response) => response.Success;

        /// <summary>
        /// Returns <c>true</c> when the generic response indicates success; otherwise <c>false</c>.
        /// </summary>
        public static bool IsSuccessful<T>(this ApiResponse<T> response) => response.Success;

        /// <summary>
        /// Returns the response message, or an empty string when the message is <c>null</c>.
        /// </summary>
        public static string GetMessageOrDefault(this ApiResponse response) =>
            response.Message ?? string.Empty;

        /// <summary>
        /// Returns the response message, or an empty string when the message is <c>null</c>.
        /// </summary>
        public static string GetMessageOrDefault<T>(this ApiResponse<T> response) =>
            response.Message ?? string.Empty;

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the response does not indicate success.
        /// </summary>
        public static void EnsureSuccess(this ApiResponse response)
        {
            if (!response.Success)
                throw new InvalidOperationException(
                    $"API call failed with status {response.StatusCode}: {response.Message ?? "No message"}");
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the generic response does not indicate success.
        /// </summary>
        public static void EnsureSuccess<T>(this ApiResponse<T> response) => response.EnsureSuccess();

        /// <summary>
        /// Creates a new <see cref="ApiResponse{T}"/> based on a non‑generic <see cref="ApiResponse"/>,
        /// copying all metadata and injecting the supplied <paramref name="data"/>.
        /// </summary>
        public static ApiResponse<T> WithData<T>(this ApiResponse response, T data) => new ApiResponse<T>
        {
            Success = response.Success,
            Data = data,
            Message = response.Message,
            StatusCode = response.StatusCode,
            Timestamp = response.Timestamp,
            TraceId = response.TraceId
        };
    }
}
