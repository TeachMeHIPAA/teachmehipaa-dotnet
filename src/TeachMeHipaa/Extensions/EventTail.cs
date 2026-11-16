using System.Runtime.CompilerServices;

namespace TeachMeHipaa;

/// <summary>
/// A position in the Event log, iterated with <c>await foreach</c>. The log's
/// <c>nextCursor</c> is null at its current end, so the tail keeps its own
/// position in <see cref="LastEventId"/>; each iteration continues from there.
/// </summary>
/// <remarks>
/// Cancelling ends the iteration with an <see cref="OperationCanceledException"/>,
/// as everywhere in .NET; <see cref="LastEventId"/> still names the last Event
/// handed out, so catch the exception and save it to resume later.
/// </remarks>
public sealed class EventTail : IAsyncEnumerable<Event>
{
    private readonly EventService _events;
    private readonly EventTailOptions _options;
    private readonly RequestOptions? _requestOptions;
    private readonly TimeProvider _timeProvider;
    private readonly CancellationToken _cancellationToken;

    internal EventTail(
        EventService events,
        EventTailOptions options,
        RequestOptions? requestOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        _events = events;
        _options = options;
        LastEventId = options.After;
        _requestOptions = requestOptions;
        _timeProvider = timeProvider;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// The id of the last Event the tail handed out, or the
    /// <see cref="EventTailOptions.After"/> it started from. Save it to resume
    /// from the same place after a restart.
    /// </summary>
    public string? LastEventId { get; private set; }

    /// <inheritdoc />
    public IAsyncEnumerator<Event> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        TailAsync(_cancellationToken).GetAsyncEnumerator(cancellationToken);

    private async IAsyncEnumerable<Event> TailAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (true)
        {
            var list = new EventListOptions { After = LastEventId, Type = _options.Type, Limit = _options.Limit };
            await foreach (var item in _events.ListAutoPagingAsync(list, _requestOptions, cancellationToken).ConfigureAwait(false))
            {
                LastEventId = item.EventId;
                yield return item;
            }

            if (!_options.Follow)
            {
                yield break;
            }

            await TimeProviderDelay.Delay(_options.Interval, _timeProvider, cancellationToken).ConfigureAwait(false);
        }
    }
}
