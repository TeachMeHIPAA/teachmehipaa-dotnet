// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="AuditEventService.ListAsync"/>.</summary>
public sealed class AuditEventListOptions
{
    /// <summary>How many objects to return, between 1 and 100. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>The <c>nextCursor</c> from the previous page.</summary>
    public string? Cursor { get; set; }

    /// <summary>Filter to one audit-event family.</summary>
    public ListAuditEventsType? Type { get; set; }

    /// <summary>Filter to one lifecycle transition.</summary>
    public ListAuditEventsTransition? Transition { get; set; }

    /// <summary>Filter to one member (canonical subject id).</summary>
    public string? SubjectId { get; set; }

    /// <summary>Inclusive lower bound on <c>at</c> (ISO-8601 instant).</summary>
    public DateTimeOffset? Since { get; set; }

    /// <summary>Inclusive upper bound on <c>at</c> (ISO-8601 instant).</summary>
    public DateTimeOffset? Until { get; set; }
}
