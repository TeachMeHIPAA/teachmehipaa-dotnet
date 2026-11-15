// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary><c>active</c> or <c>suspended</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(PartnerStatusJsonConverter))]
public readonly record struct PartnerStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public PartnerStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>active</c>.</summary>
    public static PartnerStatus Active { get; } = new("active");

    /// <summary><c>suspended</c>.</summary>
    public static PartnerStatus Suspended { get; } = new("suspended");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class PartnerStatusJsonConverter : JsonConverter<PartnerStatus>
{
    public override PartnerStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A PartnerStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, PartnerStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
