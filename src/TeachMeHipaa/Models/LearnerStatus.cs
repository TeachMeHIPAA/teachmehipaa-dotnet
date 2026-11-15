// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>LearnerStatus</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(LearnerStatusJsonConverter))]
public readonly record struct LearnerStatus
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public LearnerStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>not_started</c>.</summary>
    public static LearnerStatus NotStarted { get; } = new("not_started");

    /// <summary><c>invite_bounced</c>.</summary>
    public static LearnerStatus InviteBounced { get; } = new("invite_bounced");

    /// <summary><c>in_progress</c>.</summary>
    public static LearnerStatus InProgress { get; } = new("in_progress");

    /// <summary><c>compliant</c>.</summary>
    public static LearnerStatus Compliant { get; } = new("compliant");

    /// <summary><c>expiring_soon</c>.</summary>
    public static LearnerStatus ExpiringSoon { get; } = new("expiring_soon");

    /// <summary><c>expired</c>.</summary>
    public static LearnerStatus Expired { get; } = new("expired");

    /// <summary><c>removed</c>.</summary>
    public static LearnerStatus Removed { get; } = new("removed");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerStatusJsonConverter : JsonConverter<LearnerStatus>
{
    public override LearnerStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A LearnerStatus must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
