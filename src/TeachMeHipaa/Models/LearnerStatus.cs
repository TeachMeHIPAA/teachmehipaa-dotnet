using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>
/// A Learner's training status in one Course. New statuses may appear at any
/// time, so this holds any string; compare against the known values.
/// </summary>
[JsonConverter(typeof(LearnerStatusJsonConverter))]
public readonly record struct LearnerStatus
{
    private readonly string? _value;

    /// <summary>Wraps a status string, known or not.</summary>
    public LearnerStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Invited, not yet started.</summary>
    public static LearnerStatus NotStarted { get; } = new("not_started");

    /// <summary>The invite email bounced.</summary>
    public static LearnerStatus InviteBounced { get; } = new("invite_bounced");

    /// <summary>Started, not yet complete.</summary>
    public static LearnerStatus InProgress { get; } = new("in_progress");

    /// <summary>Holds a current Certificate.</summary>
    public static LearnerStatus Compliant { get; } = new("compliant");

    /// <summary>The Certificate expires within 60 days.</summary>
    public static LearnerStatus ExpiringSoon { get; } = new("expiring_soon");

    /// <summary>The Certificate has expired.</summary>
    public static LearnerStatus Expired { get; } = new("expired");

    /// <summary>Removed from the Organization.</summary>
    public static LearnerStatus Removed { get; } = new("removed");

    /// <summary>The status as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class LearnerStatusJsonConverter : JsonConverter<LearnerStatus>
{
    public override LearnerStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A Learner status must be a string."));

    public override void Write(Utf8JsonWriter writer, LearnerStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
