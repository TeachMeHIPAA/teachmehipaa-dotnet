// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>EventType</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(EventTypeJsonConverter))]
public readonly record struct EventType
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public EventType(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>enrollment.certified</c>.</summary>
    public static EventType EnrollmentCertified { get; } = new("enrollment.certified");

    /// <summary><c>seat.consumed</c>.</summary>
    public static EventType SeatConsumed { get; } = new("seat.consumed");

    /// <summary><c>seat.freed</c>.</summary>
    public static EventType SeatFreed { get; } = new("seat.freed");

    /// <summary><c>learner.invited</c>.</summary>
    public static EventType LearnerInvited { get; } = new("learner.invited");

    /// <summary><c>learner.started</c>.</summary>
    public static EventType LearnerStarted { get; } = new("learner.started");

    /// <summary><c>learner.certified</c>.</summary>
    public static EventType LearnerCertified { get; } = new("learner.certified");

    /// <summary><c>learner.expiring</c>.</summary>
    public static EventType LearnerExpiring { get; } = new("learner.expiring");

    /// <summary><c>learner.expired</c>.</summary>
    public static EventType LearnerExpired { get; } = new("learner.expired");

    /// <summary><c>learner.removed</c>.</summary>
    public static EventType LearnerRemoved { get; } = new("learner.removed");

    /// <summary><c>invite.bounced</c>.</summary>
    public static EventType InviteBounced { get; } = new("invite.bounced");

    /// <summary><c>certificate.revoked</c>.</summary>
    public static EventType CertificateRevoked { get; } = new("certificate.revoked");

    /// <summary><c>seats.low</c>.</summary>
    public static EventType SeatsLow { get; } = new("seats.low");

    /// <summary><c>seat_order.paid</c>.</summary>
    public static EventType SeatOrderPaid { get; } = new("seat_order.paid");

    /// <summary><c>seat_order.failed</c>.</summary>
    public static EventType SeatOrderFailed { get; } = new("seat_order.failed");

    /// <summary><c>seat_order.expired</c>.</summary>
    public static EventType SeatOrderExpired { get; } = new("seat_order.expired");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class EventTypeJsonConverter : JsonConverter<EventType>
{
    public override EventType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A EventType must be a string."));

    public override void Write(Utf8JsonWriter writer, EventType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
