// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="MemberService.ListAsync"/>.</summary>
public sealed class MemberListOptions
{
    /// <summary>How many objects to return, between 1 and 1000. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>The <c>nextCursor</c> from the previous page.</summary>
    public string? Cursor { get; set; }

    /// <summary>Opt-in, comma-separated. With <c>enrollmentSubject</c>, a member whose <c>subjectId</c> is null carries <c>enrollmentSubjectId</c>: the subject their email resolves to (a never-signed-in invitee), or null when none. Use it only to match the member to Enrollments; it is not an address for member routes. With <c>holdsInvitation</c>, each learner carries <c>holdsInvitation</c>: whether the organization has an invitation for their email or a Seat bound to them. Each key is omitted when its lookup failed.</summary>
    public string? Include { get; set; }

    /// <summary>Only admins (paginated).</summary>
    public ListMembersRole? Role { get; set; }
}
