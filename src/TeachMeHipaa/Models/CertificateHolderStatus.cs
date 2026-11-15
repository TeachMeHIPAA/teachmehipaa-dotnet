// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Whether the holder could be resolved: <c>resolved</c>, <c>none</c> or <c>lookup_failed</c>.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(CertificateHolderStatusJsonConverter))]
public readonly record struct CertificateHolderStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public CertificateHolderStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>resolved</c>.</summary>
    public static CertificateHolderStatus Resolved { get; } = new("resolved");

    /// <summary><c>none</c>.</summary>
    public static CertificateHolderStatus None { get; } = new("none");

    /// <summary><c>lookup_failed</c>.</summary>
    public static CertificateHolderStatus LookupFailed { get; } = new("lookup_failed");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class CertificateHolderStatusJsonConverter : JsonConverter<CertificateHolderStatus>
{
    public override CertificateHolderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A CertificateHolderStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, CertificateHolderStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
