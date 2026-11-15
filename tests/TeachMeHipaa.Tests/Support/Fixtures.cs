using Microsoft.Extensions.Time.Testing;

namespace TeachMeHipaa.Tests.Support;

public static class Fixtures
{
    public const string ClientId = "cli_01JD7Z3M1V4Q8T2K5N9R6W0XYZ";
    public const string ClientSecret = "tmh_cs_c2VjcmV0LXNlY3JldC1zZWNyZXQtc2VjcmV0LXM";
    public const string OrgId = "org_acme";
    public const string LearnerId = "sub_01JD80AB2C3D4E5F6G7H8J9K0M";

    public const string TokenRoute = "POST /v1/auth/token";
    public const string IntrospectionRoute = "GET /v1/auth/introspection";
    public const string LearnerRoute = $"GET /v1/organizations/{OrgId}/learners/{LearnerId}";

    /// <summary><c>dev#/components/schemas/ClientCredentialsTokenResponse</c>, as in api-sdk-surface.md §2.1.</summary>
    public static ScriptedReply TokenReply(string accessToken = "access-token-1", int expiresIn = 900) => new()
    {
        Json = $$"""
            {"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":{{expiresIn}},"scope":"learners:read learners:write"}
            """,
        Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store", ["Pragma"] = "no-cache" },
    };

    /// <summary><c>dev#/components/schemas/TokenIntrospection</c> for an API client.</summary>
    public static ScriptedReply IntrospectionReply(string? orgId = OrgId) => new()
    {
        Json = $$"""
            {"actorType":"client","clientId":"{{ClientId}}","subjectId":null,"orgId":{{(orgId is null ? "null" : $"\"{orgId}\"")}},"scopes":["learners:read","learners:write"],"mode":"live","expiresAt":"2026-11-15T12:15:00.000Z"}
            """,
    };

    /// <summary>A <c>dev#/components/schemas/LearnerDto</c>.</summary>
    public const string LearnerJson = $$"""
        {
          "learnerId": "{{LearnerId}}",
          "email": "ana@example.com",
          "name": "Ana Example",
          "externalId": "hr-123",
          "status": "in_progress",
          "course": { "courseId": "hipaa-2026", "title": "HIPAA Training 2026" },
          "invitedAt": "2026-11-01T09:00:00.000Z",
          "startedAt": "2026-11-02T10:30:00.000Z",
          "completedAt": null,
          "dueAt": "2026-12-01T00:00:00.000Z",
          "certificate": null
        }
        """;

    public static ScriptedReply LearnerReply(IReadOnlyDictionary<string, string>? headers = null) => new()
    {
        Json = LearnerJson,
        Headers = headers ?? new Dictionary<string, string>(),
    };

    public static ScriptedReply ProblemReply(int status, string json, IReadOnlyDictionary<string, string>? headers = null)
    {
        var all = new Dictionary<string, string>(headers ?? new Dictionary<string, string>())
        {
            ["Content-Type"] = "application/problem+json",
        };
        return new() { Status = (System.Net.HttpStatusCode)status, Json = json, Headers = all };
    }

    public static TeachMeHipaaClient NewClient(
        FakeHttpMessageHandler handler,
        string? organizationId = OrgId,
        FakeTimeProvider? time = null,
        int maxRetries = 2) =>
        new(new TeachMeHipaaClientOptions
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            OrganizationId = organizationId,
            HttpMessageHandler = handler,
            TimeProvider = time,
            MaxRetries = maxRetries,
        });
}
