// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The API's <c>AccessTokenResponseScopesItem</c> values.</summary>
/// <remarks>
/// New values may appear at any time, so this holds any string; compare
/// against the known values.
/// </remarks>
[JsonConverter(typeof(AccessTokenResponseScopesItemJsonConverter))]
public readonly record struct AccessTokenResponseScopesItem
{
    private readonly string? _value;

    /// <summary>Wraps a value, known or not.</summary>
    /// <param name="value">The value as the API sends it.</param>
    public AccessTokenResponseScopesItem(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary><c>me</c>.</summary>
    public static AccessTokenResponseScopesItem Me { get; } = new("me");

    /// <summary><c>enrollments:read</c>.</summary>
    public static AccessTokenResponseScopesItem EnrollmentsRead { get; } = new("enrollments:read");

    /// <summary><c>enrollments:write</c>.</summary>
    public static AccessTokenResponseScopesItem EnrollmentsWrite { get; } = new("enrollments:write");

    /// <summary><c>members:read</c>.</summary>
    public static AccessTokenResponseScopesItem MembersRead { get; } = new("members:read");

    /// <summary><c>members:admin</c>.</summary>
    public static AccessTokenResponseScopesItem MembersAdmin { get; } = new("members:admin");

    /// <summary><c>certificates:read</c>.</summary>
    public static AccessTokenResponseScopesItem CertificatesRead { get; } = new("certificates:read");

    /// <summary><c>certificates:write</c>.</summary>
    public static AccessTokenResponseScopesItem CertificatesWrite { get; } = new("certificates:write");

    /// <summary><c>billing:write</c>.</summary>
    public static AccessTokenResponseScopesItem BillingWrite { get; } = new("billing:write");

    /// <summary><c>org:admin</c>.</summary>
    public static AccessTokenResponseScopesItem OrgAdmin { get; } = new("org:admin");

    /// <summary><c>audit:read</c>.</summary>
    public static AccessTokenResponseScopesItem AuditRead { get; } = new("audit:read");

    /// <summary><c>scim</c>.</summary>
    public static AccessTokenResponseScopesItem Scim { get; } = new("scim");

    /// <summary><c>support:write</c>.</summary>
    public static AccessTokenResponseScopesItem SupportWrite { get; } = new("support:write");

    /// <summary><c>learners:read</c>.</summary>
    public static AccessTokenResponseScopesItem LearnersRead { get; } = new("learners:read");

    /// <summary><c>learners:write</c>.</summary>
    public static AccessTokenResponseScopesItem LearnersWrite { get; } = new("learners:write");

    /// <summary><c>compliance:read</c>.</summary>
    public static AccessTokenResponseScopesItem ComplianceRead { get; } = new("compliance:read");

    /// <summary><c>webhooks:manage</c>.</summary>
    public static AccessTokenResponseScopesItem WebhooksManage { get; } = new("webhooks:manage");

    /// <summary>The value as the API sends it.</summary>
    public string Value => _value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal sealed class AccessTokenResponseScopesItemJsonConverter : JsonConverter<AccessTokenResponseScopesItem>
{
    public override AccessTokenResponseScopesItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("A AccessTokenResponseScopesItem must be a string."));

    public override void Write(Utf8JsonWriter writer, AccessTokenResponseScopesItem value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
