using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;

namespace TeachMeHipaa.Tests.Support;

/// <summary>
/// Fake time that a test can run a call through without waiting: whenever the
/// call is parked on a timer, <see cref="RunAsync{T}"/> jumps the clock to it.
/// Every timer that fires is recorded, so a test can read back the backoff
/// delays and timeouts the client waited for.
/// </summary>
public sealed class TestTime : FakeTimeProvider
{
    // Only timers this short are jumped to, so the 80 s default timeout of a
    // request that is briefly in flight on another thread never fires by
    // accident. Retry delays never exceed 60 s.
    private static readonly TimeSpan LongestJump = TimeSpan.FromSeconds(60);

    private readonly ConcurrentDictionary<RecordingTimer, byte> _pending = new();
    private readonly ConcurrentQueue<TimeSpan> _fired = new();

    public TestTime()
        : base(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero))
    {
    }

    /// <summary>How long each timer that fired had been set for, in order.</summary>
    public IReadOnlyList<TimeSpan> Fired => _fired.ToArray();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new RecordingTimer(this);
        // Scheduled first: a zero due time fires inside base.CreateTimer.
        timer.Schedule(dueTime);
        timer.Inner = base.CreateTimer(_ => timer.Fire(callback, state), state, dueTime, period);
        return timer;
    }

    public async Task<T> RunAsync<T>(Task<T> call)
    {
        await RunAsync((Task)call);
        return await call;
    }

    public async Task RunAsync(Task call)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!call.IsCompleted)
        {
            var next = _pending.Keys
                .Where(timer => timer.DueTime <= LongestJump)
                .OrderBy(timer => timer.DueAt)
                .FirstOrDefault();
            if (next is not null)
            {
                var wait = next.DueAt - GetUtcNow();
                Advance(wait > TimeSpan.Zero ? wait : TimeSpan.Zero);
                continue;
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("The call never completed and is waiting on no short timer.");
            }

            await Task.Delay(1);
        }

        await call;
    }

    private sealed class RecordingTimer(TestTime time) : ITimer
    {
        public ITimer Inner { get; set; } = null!;

        public TimeSpan DueTime { get; private set; }

        public DateTimeOffset DueAt { get; private set; }

        public void Schedule(TimeSpan dueTime)
        {
            DueTime = dueTime;
            DueAt = time.GetUtcNow() + dueTime;
            if (dueTime == Timeout.InfiniteTimeSpan)
            {
                time._pending.TryRemove(this, out _);
            }
            else
            {
                time._pending[this] = 0;
            }
        }

        public void Fire(TimerCallback callback, object? state)
        {
            time._pending.TryRemove(this, out _);
            time._fired.Enqueue(DueTime);
            callback(state);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            Schedule(dueTime);
            return Inner.Change(dueTime, period);
        }

        public void Dispose()
        {
            time._pending.TryRemove(this, out _);
            Inner.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            time._pending.TryRemove(this, out _);
            return Inner.DisposeAsync();
        }
    }
}
