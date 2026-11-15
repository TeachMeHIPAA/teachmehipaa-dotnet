// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.CreateAsync"/>.</summary>
public sealed class LearnerCreateOptions
{
    /// <summary>The learner's email address. Required.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>The learner's full name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Your own id for the learner, unique within the organization.</summary>
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    /// <summary>The course to invite them to. Defaults to the organization's default course.</summary>
    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }

    /// <summary><c>email</c> (default) sends the invite email. <c>none</c> sends nothing and returns <c>joinUrl</c> for you to deliver.</summary>
    [JsonPropertyName("delivery")]
    public LearnerCreateRequestDelivery? Delivery { get; set; }

    /// <summary><c>require_available</c> fails with <c>409</c> when no seat is free.</summary>
    [JsonPropertyName("seatPolicy")]
    public LearnerCreateRequestSeatPolicy? SeatPolicy { get; set; }
}
