// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>Event</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="EventCertificateRevoked"/>, <see cref="EventEnrollmentCertified"/>, <see cref="EventInviteBounced"/>, <see cref="EventLearnerCertified"/>, <see cref="EventLearnerExpired"/>, <see cref="EventLearnerExpiring"/>, <see cref="EventLearnerInvited"/>, <see cref="EventLearnerRemoved"/>, <see cref="EventLearnerStarted"/>, <see cref="EventSeatConsumed"/>, <see cref="EventSeatFreed"/>, <see cref="EventSeatOrderExpired"/>, <see cref="EventSeatOrderFailed"/>, <see cref="EventSeatOrderPaid"/>, <see cref="EventSeatsLow"/>, <see cref="UnknownEvent"/>.
/// Chosen by <c>eventType</c>; any other value reads as <see cref="UnknownEvent"/>.
/// </remarks>
[JsonConverter(typeof(EventJsonConverter))]
public abstract record Event : TeachMeHipaaEntity
{
    private protected Event()
    {
    }
}

internal sealed class EventJsonConverter : JsonConverter<Event>
{
    public override Event? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("eventType", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "certificate.revoked" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventCertificateRevoked),
            "enrollment.certified" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventEnrollmentCertified),
            "invite.bounced" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventInviteBounced),
            "learner.certified" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerCertified),
            "learner.expired" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerExpired),
            "learner.expiring" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerExpiring),
            "learner.invited" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerInvited),
            "learner.removed" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerRemoved),
            "learner.started" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventLearnerStarted),
            "seat.consumed" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatConsumed),
            "seat.freed" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatFreed),
            "seat_order.expired" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatOrderExpired),
            "seat_order.failed" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatOrderFailed),
            "seat_order.paid" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatOrderPaid),
            "seats.low" => json.Deserialize(TeachMeHipaaJsonContext.Default.EventSeatsLow),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownEvent),
        };
    }

    public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case EventCertificateRevoked member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventCertificateRevoked);
                break;
            case EventEnrollmentCertified member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventEnrollmentCertified);
                break;
            case EventInviteBounced member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventInviteBounced);
                break;
            case EventLearnerCertified member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerCertified);
                break;
            case EventLearnerExpired member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerExpired);
                break;
            case EventLearnerExpiring member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerExpiring);
                break;
            case EventLearnerInvited member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerInvited);
                break;
            case EventLearnerRemoved member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerRemoved);
                break;
            case EventLearnerStarted member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventLearnerStarted);
                break;
            case EventSeatConsumed member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatConsumed);
                break;
            case EventSeatFreed member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatFreed);
                break;
            case EventSeatOrderExpired member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatOrderExpired);
                break;
            case EventSeatOrderFailed member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatOrderFailed);
                break;
            case EventSeatOrderPaid member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatOrderPaid);
                break;
            case EventSeatsLow member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.EventSeatsLow);
                break;
            case UnknownEvent member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownEvent);
                break;
        }
    }
}
