// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EnrollmentService.BatchByFilterAsync"/>.</summary>
public sealed class EnrollmentBatchByFilterOptions
{
    /// <summary>What to do to each enrollment: <c>remind</c>, <c>resendInvite</c>, <c>cancel</c>, <c>recertify</c>, <c>archive</c> or <c>unarchive</c>. Required.</summary>
    [JsonPropertyName("operation")]
    public EnrollmentsBatchRequestOperation Operation { get; set; }

    /// <summary>The enrollments to act on: a list of <c>enrollmentIds</c> (up to 500), or a <c>filter</c>. Required.</summary>
    [JsonPropertyName("select")]
    public EnrollmentsBatchRequestSelectFilter Select { get; set; } = new();
}
