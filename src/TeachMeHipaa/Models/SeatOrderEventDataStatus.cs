// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>SeatOrderEventDataStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(SeatOrderEventDataStatusJsonConverter))]
public readonly record struct SeatOrderEventDataStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public SeatOrderEventDataStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>paid</c>.</summary>
    public static SeatOrderEventDataStatus Paid { get; } = new("paid");

    /// <summary><c>failed</c>.</summary>
    public static SeatOrderEventDataStatus Failed { get; } = new("failed");

    /// <summary><c>expired</c>.</summary>
    public static SeatOrderEventDataStatus Expired { get; } = new("expired");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class SeatOrderEventDataStatusJsonConverter : JsonConverter<SeatOrderEventDataStatus>
{
    public override SeatOrderEventDataStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A SeatOrderEventDataStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, SeatOrderEventDataStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
