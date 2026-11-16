namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="EventService.TailAsync"/>.</summary>
public sealed class EventTailOptions
{
    /// <summary>
    /// Yield only Events after this id (exclusive), such as a saved
    /// <see cref="EventTail.LastEventId"/>. Null starts at the oldest retained Event.
    /// </summary>
    public string? After { get; set; }

    /// <summary>Only Events of this type.</summary>
    public EventType? Type { get; set; }

    /// <summary>How many Events to fetch per request, between 1 and 100; the API checks the range.</summary>
    public long? Limit { get; set; }

    /// <summary>Once caught up, keep polling for new Events instead of ending.</summary>
    public bool Follow { get; set; }

    /// <summary>How long to wait between polls once caught up in <see cref="Follow"/> mode. Defaults to 2 seconds.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(2);
}
