// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>PartnerOrganizationCreateScopesItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(PartnerOrganizationCreateScopesItemJsonConverter))]
public readonly record struct PartnerOrganizationCreateScopesItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public PartnerOrganizationCreateScopesItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>enrollments:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem EnrollmentsRead { get; } = new("enrollments:read");

    /// <summary><c>enrollments:write</c>.</summary>
    public static PartnerOrganizationCreateScopesItem EnrollmentsWrite { get; } = new("enrollments:write");

    /// <summary><c>certificates:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem CertificatesRead { get; } = new("certificates:read");

    /// <summary><c>members:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem MembersRead { get; } = new("members:read");

    /// <summary><c>audit:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem AuditRead { get; } = new("audit:read");

    /// <summary><c>learners:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem LearnersRead { get; } = new("learners:read");

    /// <summary><c>learners:write</c>.</summary>
    public static PartnerOrganizationCreateScopesItem LearnersWrite { get; } = new("learners:write");

    /// <summary><c>compliance:read</c>.</summary>
    public static PartnerOrganizationCreateScopesItem ComplianceRead { get; } = new("compliance:read");

    /// <summary><c>seats:purchase</c>.</summary>
    public static PartnerOrganizationCreateScopesItem SeatsPurchase { get; } = new("seats:purchase");

    /// <summary><c>webhooks:manage</c>.</summary>
    public static PartnerOrganizationCreateScopesItem WebhooksManage { get; } = new("webhooks:manage");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class PartnerOrganizationCreateScopesItemJsonConverter : JsonConverter<PartnerOrganizationCreateScopesItem>
{
    public override PartnerOrganizationCreateScopesItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A PartnerOrganizationCreateScopesItem must be a string."));

    public override void Write(Utf8JsonWriter writer, PartnerOrganizationCreateScopesItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
