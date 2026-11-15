// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>One of the API's kinds of <c>AuditEvent</c>.</summary>
/// <remarks>
/// Known kinds: <see cref="AuditEventApiClientCreated"/>, <see cref="AuditEventApiClientRevoked"/>, <see cref="AuditEventApiClientSecretRotated"/>, <see cref="AuditEventApiClientUpdated"/>, <see cref="AuditEventCertificateIssued"/>, <see cref="AuditEventCertificateRevoked"/>, <see cref="AuditEventEnrollmentTransition"/>, <see cref="AuditEventLearnerLaunchLinkCreated"/>, <see cref="AuditEventLearnerUpdated"/>, <see cref="AuditEventMemberRemoved"/>, <see cref="AuditEventMemberRoleChanged"/>, <see cref="AuditEventMemberTransferred"/>, <see cref="AuditEventOauthGrantCreated"/>, <see cref="AuditEventOauthGrantRevoked"/>, <see cref="AuditEventOauthGrantUpdated"/>, <see cref="AuditEventOrganizationClosed"/>, <see cref="AuditEventOrganizationCreatedByPartner"/>, <see cref="AuditEventOrganizationReopened"/>, <see cref="AuditEventOrganizationUpdated"/>, <see cref="AuditEventSeatGranted"/>, <see cref="AuditEventSeatOrderCreated"/>, <see cref="AuditEventSeatOrderFailed"/>, <see cref="AuditEventSeatOrderPaid"/>, <see cref="UnknownAuditEvent"/>.
/// Chosen by <c>type</c>; any other value reads as <see cref="UnknownAuditEvent"/>.
/// </remarks>
[JsonConverter(typeof(AuditEventJsonConverter))]
public abstract record AuditEvent : TeachMeHipaaEntity
{
    private protected AuditEvent()
    {
    }
}

internal sealed class AuditEventJsonConverter : JsonConverter<AuditEvent>
{
    public override AuditEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement;
        var tag = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("type", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        return tag switch
        {
            "api_client.created" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventApiClientCreated),
            "api_client.revoked" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventApiClientRevoked),
            "api_client.secret_rotated" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventApiClientSecretRotated),
            "api_client.updated" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventApiClientUpdated),
            "certificate.issued" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventCertificateIssued),
            "certificate.revoked" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventCertificateRevoked),
            "enrollment.transition" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventEnrollmentTransition),
            "learner.launch_link_created" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventLearnerLaunchLinkCreated),
            "learner.updated" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventLearnerUpdated),
            "member.removed" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventMemberRemoved),
            "member.role_changed" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventMemberRoleChanged),
            "member.transferred" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventMemberTransferred),
            "oauth_grant.created" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOauthGrantCreated),
            "oauth_grant.revoked" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOauthGrantRevoked),
            "oauth_grant.updated" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOauthGrantUpdated),
            "organization.closed" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOrganizationClosed),
            "organization.created_by_partner" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOrganizationCreatedByPartner),
            "organization.reopened" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOrganizationReopened),
            "organization.updated" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventOrganizationUpdated),
            "seat.granted" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventSeatGranted),
            "seat_order.created" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventSeatOrderCreated),
            "seat_order.failed" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventSeatOrderFailed),
            "seat_order.paid" => json.Deserialize(TeachMeHipaaJsonContext.Default.AuditEventSeatOrderPaid),
            _ => json.Deserialize(TeachMeHipaaJsonContext.Default.UnknownAuditEvent),
        };
    }

    public override void Write(Utf8JsonWriter writer, AuditEvent value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case AuditEventApiClientCreated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventApiClientCreated);
                break;
            case AuditEventApiClientRevoked member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventApiClientRevoked);
                break;
            case AuditEventApiClientSecretRotated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventApiClientSecretRotated);
                break;
            case AuditEventApiClientUpdated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventApiClientUpdated);
                break;
            case AuditEventCertificateIssued member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventCertificateIssued);
                break;
            case AuditEventCertificateRevoked member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventCertificateRevoked);
                break;
            case AuditEventEnrollmentTransition member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventEnrollmentTransition);
                break;
            case AuditEventLearnerLaunchLinkCreated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventLearnerLaunchLinkCreated);
                break;
            case AuditEventLearnerUpdated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventLearnerUpdated);
                break;
            case AuditEventMemberRemoved member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventMemberRemoved);
                break;
            case AuditEventMemberRoleChanged member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventMemberRoleChanged);
                break;
            case AuditEventMemberTransferred member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventMemberTransferred);
                break;
            case AuditEventOauthGrantCreated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOauthGrantCreated);
                break;
            case AuditEventOauthGrantRevoked member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOauthGrantRevoked);
                break;
            case AuditEventOauthGrantUpdated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOauthGrantUpdated);
                break;
            case AuditEventOrganizationClosed member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOrganizationClosed);
                break;
            case AuditEventOrganizationCreatedByPartner member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOrganizationCreatedByPartner);
                break;
            case AuditEventOrganizationReopened member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOrganizationReopened);
                break;
            case AuditEventOrganizationUpdated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventOrganizationUpdated);
                break;
            case AuditEventSeatGranted member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventSeatGranted);
                break;
            case AuditEventSeatOrderCreated member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventSeatOrderCreated);
                break;
            case AuditEventSeatOrderFailed member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventSeatOrderFailed);
                break;
            case AuditEventSeatOrderPaid member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.AuditEventSeatOrderPaid);
                break;
            case UnknownAuditEvent member:
                JsonSerializer.Serialize(writer, member, TeachMeHipaaJsonContext.Default.UnknownAuditEvent);
                break;
        }
    }
}
