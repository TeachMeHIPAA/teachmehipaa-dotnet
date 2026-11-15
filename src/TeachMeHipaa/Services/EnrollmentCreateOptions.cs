// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.CreateAsync"/>.</summary>
public sealed class EnrollmentCreateOptions
{
    /// <summary>The learner's email address. Required.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The course to invite them to. Defaults to <c>hipaa</c>.</summary>
    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }
}
