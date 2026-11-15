// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>ListAuditEventsType</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(ListAuditEventsTypeJsonConverter))]
public readonly record struct ListAuditEventsType
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public ListAuditEventsType(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>enrollment.transition</c>.</summary>
    public static ListAuditEventsType EnrollmentTransition { get; } = new("enrollment.transition");

    /// <summary><c>organization.updated</c>.</summary>
    public static ListAuditEventsType OrganizationUpdated { get; } = new("organization.updated");

    /// <summary><c>organization.closed</c>.</summary>
    public static ListAuditEventsType OrganizationClosed { get; } = new("organization.closed");

    /// <summary><c>organization.reopened</c>.</summary>
    public static ListAuditEventsType OrganizationReopened { get; } = new("organization.reopened");

    /// <summary><c>member.role_changed</c>.</summary>
    public static ListAuditEventsType MemberRoleChanged { get; } = new("member.role_changed");

    /// <summary><c>member.transferred</c>.</summary>
    public static ListAuditEventsType MemberTransferred { get; } = new("member.transferred");

    /// <summary><c>member.removed</c>.</summary>
    public static ListAuditEventsType MemberRemoved { get; } = new("member.removed");

    /// <summary><c>certificate.issued</c>.</summary>
    public static ListAuditEventsType CertificateIssued { get; } = new("certificate.issued");

    /// <summary><c>certificate.revoked</c>.</summary>
    public static ListAuditEventsType CertificateRevoked { get; } = new("certificate.revoked");

    /// <summary><c>seat.granted</c>.</summary>
    public static ListAuditEventsType SeatGranted { get; } = new("seat.granted");

    /// <summary><c>api_client.created</c>.</summary>
    public static ListAuditEventsType ApiClientCreated { get; } = new("api_client.created");

    /// <summary><c>api_client.secret_rotated</c>.</summary>
    public static ListAuditEventsType ApiClientSecretRotated { get; } = new("api_client.secret_rotated");

    /// <summary><c>api_client.revoked</c>.</summary>
    public static ListAuditEventsType ApiClientRevoked { get; } = new("api_client.revoked");

    /// <summary><c>learner.updated</c>.</summary>
    public static ListAuditEventsType LearnerUpdated { get; } = new("learner.updated");

    /// <summary><c>oauth_grant.created</c>.</summary>
    public static ListAuditEventsType OauthGrantCreated { get; } = new("oauth_grant.created");

    /// <summary><c>oauth_grant.revoked</c>.</summary>
    public static ListAuditEventsType OauthGrantRevoked { get; } = new("oauth_grant.revoked");

    /// <summary><c>seat_order.created</c>.</summary>
    public static ListAuditEventsType SeatOrderCreated { get; } = new("seat_order.created");

    /// <summary><c>seat_order.paid</c>.</summary>
    public static ListAuditEventsType SeatOrderPaid { get; } = new("seat_order.paid");

    /// <summary><c>seat_order.failed</c>.</summary>
    public static ListAuditEventsType SeatOrderFailed { get; } = new("seat_order.failed");

    /// <summary><c>learner.launch_link_created</c>.</summary>
    public static ListAuditEventsType LearnerLaunchLinkCreated { get; } = new("learner.launch_link_created");

    /// <summary><c>api_client.updated</c>.</summary>
    public static ListAuditEventsType ApiClientUpdated { get; } = new("api_client.updated");

    /// <summary><c>oauth_grant.updated</c>.</summary>
    public static ListAuditEventsType OauthGrantUpdated { get; } = new("oauth_grant.updated");

    /// <summary><c>organization.created_by_partner</c>.</summary>
    public static ListAuditEventsType OrganizationCreatedByPartner { get; } = new("organization.created_by_partner");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class ListAuditEventsTypeJsonConverter : JsonConverter<ListAuditEventsType>
{
    public override ListAuditEventsType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A ListAuditEventsType must be a string."));

    public override void Write(Utf8JsonWriter writer, ListAuditEventsType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
