// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EventService.ListAsync"/>.</summary>
public sealed class EventListOptions
{
    /// <summary>How many objects to return, between 1 and 100. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>Return events after this event id (exclusive). The previous page's <c>nextCursor</c>.</summary>
    public string? After { get; set; }

    /// <summary>Filter to one event type.</summary>
    public EventType? Type { get; set; }
}
