// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>TokenIntrospectionScopesItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(TokenIntrospectionScopesItemJsonConverter))]
public readonly record struct TokenIntrospectionScopesItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public TokenIntrospectionScopesItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>me</c>.</summary>
    public static TokenIntrospectionScopesItem Me { get; } = new("me");

    /// <summary><c>enrollments:read</c>.</summary>
    public static TokenIntrospectionScopesItem EnrollmentsRead { get; } = new("enrollments:read");

    /// <summary><c>enrollments:write</c>.</summary>
    public static TokenIntrospectionScopesItem EnrollmentsWrite { get; } = new("enrollments:write");

    /// <summary><c>members:read</c>.</summary>
    public static TokenIntrospectionScopesItem MembersRead { get; } = new("members:read");

    /// <summary><c>members:admin</c>.</summary>
    public static TokenIntrospectionScopesItem MembersAdmin { get; } = new("members:admin");

    /// <summary><c>certificates:read</c>.</summary>
    public static TokenIntrospectionScopesItem CertificatesRead { get; } = new("certificates:read");

    /// <summary><c>certificates:write</c>.</summary>
    public static TokenIntrospectionScopesItem CertificatesWrite { get; } = new("certificates:write");

    /// <summary><c>billing:write</c>.</summary>
    public static TokenIntrospectionScopesItem BillingWrite { get; } = new("billing:write");

    /// <summary><c>org:admin</c>.</summary>
    public static TokenIntrospectionScopesItem OrgAdmin { get; } = new("org:admin");

    /// <summary><c>audit:read</c>.</summary>
    public static TokenIntrospectionScopesItem AuditRead { get; } = new("audit:read");

    /// <summary><c>scim</c>.</summary>
    public static TokenIntrospectionScopesItem Scim { get; } = new("scim");

    /// <summary><c>support:write</c>.</summary>
    public static TokenIntrospectionScopesItem SupportWrite { get; } = new("support:write");

    /// <summary><c>learners:read</c>.</summary>
    public static TokenIntrospectionScopesItem LearnersRead { get; } = new("learners:read");

    /// <summary><c>learners:write</c>.</summary>
    public static TokenIntrospectionScopesItem LearnersWrite { get; } = new("learners:write");

    /// <summary><c>compliance:read</c>.</summary>
    public static TokenIntrospectionScopesItem ComplianceRead { get; } = new("compliance:read");

    /// <summary><c>seats:purchase</c>.</summary>
    public static TokenIntrospectionScopesItem SeatsPurchase { get; } = new("seats:purchase");

    /// <summary><c>webhooks:manage</c>.</summary>
    public static TokenIntrospectionScopesItem WebhooksManage { get; } = new("webhooks:manage");

    /// <summary><c>partner:orgs:read</c>.</summary>
    public static TokenIntrospectionScopesItem PartnerOrgsRead { get; } = new("partner:orgs:read");

    /// <summary><c>partner:orgs:write</c>.</summary>
    public static TokenIntrospectionScopesItem PartnerOrgsWrite { get; } = new("partner:orgs:write");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class TokenIntrospectionScopesItemJsonConverter : JsonConverter<TokenIntrospectionScopesItem>
{
    public override TokenIntrospectionScopesItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A TokenIntrospectionScopesItem must be a string."));

    public override void Write(Utf8JsonWriter writer, TokenIntrospectionScopesItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
