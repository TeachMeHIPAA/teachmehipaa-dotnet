// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventMemberRoleChangedRoleFrom</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventMemberRoleChangedRoleFromJsonConverter))]
public readonly record struct AuditEventMemberRoleChangedRoleFrom
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventMemberRoleChangedRoleFrom(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>admin</c>.</summary>
    public static AuditEventMemberRoleChangedRoleFrom Admin { get; } = new("admin");

    /// <summary><c>learner</c>.</summary>
    public static AuditEventMemberRoleChangedRoleFrom Learner { get; } = new("learner");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventMemberRoleChangedRoleFromJsonConverter : JsonConverter<AuditEventMemberRoleChangedRoleFrom>
{
    public override AuditEventMemberRoleChangedRoleFrom Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventMemberRoleChangedRoleFrom must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventMemberRoleChangedRoleFrom value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
