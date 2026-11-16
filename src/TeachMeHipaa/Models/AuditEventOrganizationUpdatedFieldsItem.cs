// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventOrganizationUpdatedFieldsItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventOrganizationUpdatedFieldsItemJsonConverter))]
public readonly record struct AuditEventOrganizationUpdatedFieldsItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventOrganizationUpdatedFieldsItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>name</c>.</summary>
    public static AuditEventOrganizationUpdatedFieldsItem Name { get; } = new("name");

    /// <summary><c>trainingNeeds</c>.</summary>
    public static AuditEventOrganizationUpdatedFieldsItem TrainingNeeds { get; } = new("trainingNeeds");

    /// <summary><c>snoozeDuration</c>.</summary>
    public static AuditEventOrganizationUpdatedFieldsItem SnoozeDuration { get; } = new("snoozeDuration");

    /// <summary><c>autoRecertify</c>.</summary>
    public static AuditEventOrganizationUpdatedFieldsItem AutoRecertify { get; } = new("autoRecertify");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventOrganizationUpdatedFieldsItemJsonConverter : JsonConverter<AuditEventOrganizationUpdatedFieldsItem>
{
    public override AuditEventOrganizationUpdatedFieldsItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventOrganizationUpdatedFieldsItem must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventOrganizationUpdatedFieldsItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
