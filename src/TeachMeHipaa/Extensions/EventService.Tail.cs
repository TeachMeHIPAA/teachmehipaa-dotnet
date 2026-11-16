namespace TeachMeHipaa;

public sealed partial class EventService
{
    /// <summary>Tail the event log.</summary>
    /// <remarks>
    /// Iterates the Organization's Events oldest first, after
    /// <see cref="EventTailOptions.After"/>. Once caught up the iteration
    /// ends, or with <see cref="EventTailOptions.Follow"/> it waits
    /// <see cref="EventTailOptions.Interval"/> and polls again until you break
    /// out of the loop or cancel. Nothing is fetched until you iterate.
    /// </remarks>
    /// <param name="options">Where to start, which Events and whether to follow.</param>
    /// <param name="requestOptions">Per-call settings applied to every poll, such as a timeout.</param>
    /// <param name="cancellationToken">Ends the tail, mid-wait included, with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>The tail, to iterate with <c>await foreach</c>; its <see cref="EventTail.LastEventId"/> is the position to save.</returns>
    public EventTail TailAsync(
        EventTailOptions? options = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        new(this, options ?? new EventTailOptions(), requestOptions, _requestor.TimeProvider, cancellationToken);
}
