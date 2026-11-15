namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="LearnerService.GetAsync"/>.</summary>
public sealed class LearnerGetOptions
{
    /// <summary>The Course. Defaults to the Organization's default Course.</summary>
    public string? CourseId { get; set; }
}
