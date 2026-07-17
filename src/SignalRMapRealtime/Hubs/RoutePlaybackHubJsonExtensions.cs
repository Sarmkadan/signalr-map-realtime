#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="RoutePlaybackHub"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class contains helper methods to serialize <see cref="RoutePlaybackHub"/> instances to JSON
/// and deserialize JSON strings back to <see cref="RoutePlaybackHub"/> objects.
/// </para>
/// <para>
/// The serialization uses camelCase property naming convention and includes
/// <see cref="JsonSerializerOptions"/> with default settings for the <see cref="RoutePlaybackHub"/> type.
/// </para>
/// </remarks>
public static class RoutePlaybackHubJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false
	};

	/// <summary>
	/// Serializes the specified <see cref="RoutePlaybackHub"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="RoutePlaybackHub"/> instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the <see cref="RoutePlaybackHub"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this RoutePlaybackHub value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="RoutePlaybackHub"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="RoutePlaybackHub"/> instance populated from the JSON data, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
	/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to a <see cref="RoutePlaybackHub"/>.</exception>
	public static RoutePlaybackHub? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
		}

		return JsonSerializer.Deserialize<RoutePlaybackHub>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="RoutePlaybackHub"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized <see cref="RoutePlaybackHub"/> instance if successful; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the JSON was successfully deserialized; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out RoutePlaybackHub? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		if (string.IsNullOrWhiteSpace(json))
		{
			value = null;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<RoutePlaybackHub>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}