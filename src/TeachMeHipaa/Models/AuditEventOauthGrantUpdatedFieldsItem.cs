// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventOauthGrantUpdatedFieldsItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventOauthGrantUpdatedFieldsItemJsonConverter))]
public readonly record struct AuditEventOauthGrantUpdatedFieldsItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventOauthGrantUpdatedFieldsItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>maxSeatsPerOrder</c>.</summary>
    public static AuditEventOauthGrantUpdatedFieldsItem MaxSeatsPerOrder { get; } = new("maxSeatsPerOrder");

    /// <summary><c>maxSpendPer30Days</c>.</summary>
    public static AuditEventOauthGrantUpdatedFieldsItem MaxSpendPer30Days { get; } = new("maxSpendPer30Days");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventOauthGrantUpdatedFieldsItemJsonConverter : JsonConverter<AuditEventOauthGrantUpdatedFieldsItem>
{
    public override AuditEventOauthGrantUpdatedFieldsItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventOauthGrantUpdatedFieldsItem must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventOauthGrantUpdatedFieldsItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
