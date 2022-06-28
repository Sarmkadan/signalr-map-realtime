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
		/// <param name="response">The API response to check.</param>
		/// <returns><c>true</c> when the response indicates success; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static bool IsSuccessful(this ApiResponse response)
		{
			ArgumentNullException.ThrowIfNull(response);
			return response.Success;
		}

		/// <summary>
		/// Returns <c>true</c> when the generic response indicates success; otherwise <c>false</c>.
		/// </summary>
		/// <param name="response">The API response to check.</param>
		/// <typeparam name="T">The type of data in the response.</typeparam>
		/// <returns><c>true</c> when the response indicates success; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static bool IsSuccessful<T>(this ApiResponse<T> response)
		{
			ArgumentNullException.ThrowIfNull(response);
			return response.Success;
		}

		/// <summary>
		/// Returns the response message, or an empty string when the message is <c>null</c>.
		/// </summary>
		/// <param name="response">The API response.</param>
		/// <returns>The response message or an empty string if the message is <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static string GetMessageOrDefault(this ApiResponse response)
		{
			ArgumentNullException.ThrowIfNull(response);
			return response.Message ?? string.Empty;
		}

		/// <summary>
		/// Returns the response message, or an empty string when the message is <c>null</c>.
		/// </summary>
		/// <param name="response">The API response.</param>
		/// <typeparam name="T">The type of data in the response.</typeparam>
		/// <returns>The response message or an empty string if the message is <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static string GetMessageOrDefault<T>(this ApiResponse<T> response)
		{
			ArgumentNullException.ThrowIfNull(response);
			return response.Message ?? string.Empty;
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the response does not indicate success.
		/// </summary>
		/// <param name="response">The API response to validate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The response indicates failure.</exception>
		public static void EnsureSuccess(this ApiResponse response)
		{
			ArgumentNullException.ThrowIfNull(response);
			if (!response.Success)
			{
				throw new InvalidOperationException(
					$"API call failed with status {response.StatusCode}: {response.Message ?? "No message"}");
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the generic response does not indicate success.
		/// </summary>
		/// <param name="response">The API response to validate.</param>
		/// <typeparam name="T">The type of data in the response.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The response indicates failure.</exception>
		public static void EnsureSuccess<T>(this ApiResponse<T> response)
		{
			ArgumentNullException.ThrowIfNull(response);
			response.EnsureSuccess();
		}

		/// <summary>
		/// Creates a new <see cref="ApiResponse{T}"/> based on a non‑generic <see cref="ApiResponse"/>,
		/// copying all metadata and injecting the supplied <paramref name="data"/>.
		/// </summary>
		/// <param name="response">The source API response.</param>
		/// <param name="data">The data to inject into the new response.</param>
		/// <typeparam name="T">The type of data in the new response.</typeparam>
		/// <returns>A new <see cref="ApiResponse{T}"/> with the specified data and copied metadata.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static ApiResponse<T> WithData<T>(this ApiResponse response, T data)
		{
			ArgumentNullException.ThrowIfNull(response);
			return new ApiResponse<T>
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
}