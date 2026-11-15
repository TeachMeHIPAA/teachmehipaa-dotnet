// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AuditEventOauthGrantRevokedReason</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AuditEventOauthGrantRevokedReasonJsonConverter))]
public readonly record struct AuditEventOauthGrantRevokedReason
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AuditEventOauthGrantRevokedReason(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>admin</c>.</summary>
    public static AuditEventOauthGrantRevokedReason Admin { get; } = new("admin");

    /// <summary><c>code_reuse</c>.</summary>
    public static AuditEventOauthGrantRevokedReason CodeReuse { get; } = new("code_reuse");

    /// <summary><c>refresh_token_reuse</c>.</summary>
    public static AuditEventOauthGrantRevokedReason RefreshTokenReuse { get; } = new("refresh_token_reuse");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AuditEventOauthGrantRevokedReasonJsonConverter : JsonConverter<AuditEventOauthGrantRevokedReason>
{
    public override AuditEventOauthGrantRevokedReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AuditEventOauthGrantRevokedReason must be a string."));

    public override void Write(Utf8JsonWriter writer, AuditEventOauthGrantRevokedReason value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
