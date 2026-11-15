using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.CreateAsync"/> (<c>dev#/components/schemas/LearnerCreateRequest</c>).</summary>
public sealed class LearnerCreateOptions
{
    /// <summary>The Learner's email address. Required.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>The Learner's full name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Your own id for the Learner, unique within the Organization.</summary>
    [JsonPropertyName("externalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    /// <summary>The Course to invite them to. Defaults to the Organization's default Course.</summary>
    [JsonPropertyName("courseId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CourseId { get; set; }

    /// <summary><c>email</c> (the default) sends the invite; <c>none</c> sends nothing.</summary>
    [JsonPropertyName("delivery")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Delivery { get; set; }

    /// <summary><c>require_available</c> fails with a 409 when no Seat is free.</summary>
    [JsonPropertyName("seatPolicy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SeatPolicy { get; set; }
}
