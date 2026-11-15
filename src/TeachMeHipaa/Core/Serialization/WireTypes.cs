using System.Text.Json.Serialization;

namespace TeachMeHipaa;

// Shapes the core reads but never hands to callers. Every field is optional
// here so a malformed body becomes a TeachMeHipaaException, not a JsonException.

/// <summary><c>dev#/components/schemas/ClientCredentialsTokenResponse</c>.</summary>
internal sealed class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public long? ExpiresIn { get; set; }
}

/// <summary><c>dev#/components/schemas/OAuthTokenError</c>.</summary>
internal sealed class OAuthTokenError
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}

/// <summary>The part of <c>dev#/components/schemas/TokenIntrospection</c> the Organization binding needs.</summary>
internal sealed record OrganizationIntrospection : TeachMeHipaaEntity
{
    [JsonPropertyName("orgId")]
    public string? OrgId { get; init; }
}

/// <summary><c>dev#/components/schemas/Problem</c> (RFC 9457).</summary>
internal sealed class ProblemDocument
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("errors")]
    public List<ProblemFieldError>? Errors { get; set; }
}

internal sealed class ProblemFieldError
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
