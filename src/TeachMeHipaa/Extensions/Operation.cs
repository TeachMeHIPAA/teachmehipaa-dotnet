using System.Globalization;
using System.Text;
using System.Text.Json;

namespace TeachMeHipaa;

// Work that may finish after the request that started it (grilling Q24).
// Most Operations are done when they are returned; Test deliveries and
// redrives finish later.
public sealed partial record Operation
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxInterval = TimeSpan.FromSeconds(10);

    // Strict, so a corrupt export fails here instead of handing back mangled text.
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// The client this Operation was read through, which <see cref="WaitAsync"/>
    /// polls with. Not public, so it is never serialized; and since every read
    /// has its own <see cref="TeachMeHipaaEntity.LastResponse"/>, it never makes
    /// equal reads unequal either.
    /// </summary>
    internal ApiRequestor? Requestor { get; set; }

    /// <summary>
    /// The operation id in an Operation's <see cref="Name"/>, which is
    /// <c>organizations/{orgId}/operations/{operationId}</c>: what
    /// <see cref="OperationService.GetAsync"/> takes.
    /// </summary>
    /// <param name="name">An Operation's <see cref="Name"/>.</param>
    /// <returns>The operation id.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is not an Operation name.</exception>
    public static string OperationIdFromName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        var segments = name.Split('/');
        // No list pattern: it needs System.Index, which netstandard2.0 lacks.
        if (segments.Length == 4
            && segments[0] == "organizations" && segments[1].Length > 0
            && segments[2] == "operations" && segments[3].Length > 0)
        {
            return segments[3];
        }

        throw new ArgumentException($"\"{name}\" is not an Operation name (organizations/{{orgId}}/operations/{{operationId}}).", nameof(name));
    }

    /// <summary>Polls until the Operation is done and returns its final state.</summary>
    /// <remarks>
    /// An Operation that is already done returns itself, without a request.
    /// Otherwise the first poll comes <paramref name="interval"/> after the
    /// call, and later ones back off to 10 s apart; the last one lands on the
    /// <paramref name="timeout"/>. The client's <see cref="TimeProvider"/>
    /// keeps the time.
    /// </remarks>
    /// <param name="timeout">How long to wait in all. Defaults to 5 minutes.</param>
    /// <param name="interval">How long to wait before the first poll, and the least between any two. Defaults to 1 second.</param>
    /// <param name="cancellationToken">Stops waiting, mid-delay included, with an <see cref="OperationCanceledException"/>.</param>
    /// <returns>The done Operation, with <see cref="TeachMeHipaaEntity.LastResponse"/> from the last poll.</returns>
    /// <exception cref="OperationFailedException">The Operation finished with an <see cref="Error"/>.</exception>
    /// <exception cref="OperationTimeoutException">The Operation was not done within <paramref name="timeout"/>.</exception>
    /// <exception cref="InvalidOperationException">This Operation was not returned by an API call, so there is no client to poll with.</exception>
    /// <exception cref="TeachMeHipaaException">A poll failed.</exception>
    public async Task<Operation> WaitAsync(
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default)
    {
        if (Done)
        {
            return Settled();
        }

        var limit = timeout ?? DefaultTimeout;
        var least = interval ?? DefaultInterval;
        if (limit < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), limit, "The timeout can't be negative.");
        }

        if (least <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), least, "The interval must be positive.");
        }

        var requestor = Requestor
            ?? throw new InvalidOperationException($"Operation {Name} was not returned by an API call, so it has no client to poll with.");
        var operationId = OperationIdFromName(Name);
        var operations = new OperationService(requestor);
        var time = requestor.TimeProvider;
        var started = time.GetTimestamp();
        var latest = this;
        for (var poll = 1; ; poll++)
        {
            var remaining = limit - time.GetElapsedTime(started);
            if (remaining <= TimeSpan.Zero)
            {
                throw TimedOut(latest, limit);
            }

            var delay = PollDelay(least, poll);
#if NET8_0_OR_GREATER
            await Task.Delay(delay < remaining ? delay : remaining, time, cancellationToken).ConfigureAwait(false);
#else
            await time.Delay(delay < remaining ? delay : remaining, cancellationToken).ConfigureAwait(false);
#endif
            latest = await operations.GetAsync(operationId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (latest.Done)
            {
                return latest.Settled();
            }

            // That poll was the one at the deadline.
            if (delay >= remaining)
            {
                throw TimedOut(latest, limit);
            }
        }
    }

    /// <summary>The CSV file of a done export Operation.</summary>
    /// <returns>The CSV text, its suggested file name and its row count.</returns>
    /// <exception cref="OperationFailedException">The export failed.</exception>
    /// <exception cref="InvalidOperationException">The Operation is not done, or is not a CSV export.</exception>
    public ExportResult GetExportResult()
    {
        // The spec says only that an export's response "holds the CSV file".
        // The API sends it inline as the monorepo's ExportArtifact:
        // {format: "csv", contentType, filename, rowCount, dataBase64}.
        var artifact = SettledResponse();
        if (artifact is not { ValueKind: JsonValueKind.Object } json
            || !json.TryGetProperty("format", out var format) || format.ValueKind != JsonValueKind.String || format.GetString() != "csv"
            || !json.TryGetProperty("dataBase64", out var data) || data.ValueKind != JsonValueKind.String)
        {
            throw NotProduced("a CSV export");
        }

        string csv;
        try
        {
            csv = StrictUtf8.GetString(Convert.FromBase64String(data.GetString()!));
        }
        catch (Exception error) when (error is FormatException or ArgumentException)
        {
            throw NotProduced("a CSV export");
        }

        return new ExportResult(
            csv,
            json.TryGetProperty("filename", out var fileName) && fileName.ValueKind == JsonValueKind.String ? fileName.GetString() : null,
            json.TryGetProperty("rowCount", out var rowCount) && rowCount.TryGetInt64(out var rows) ? rows : null);
    }

    /// <summary>
    /// How the Delivery of a done <c>WebhookSubscriptions.TestAsync</c> or
    /// <c>RedriveAsync</c> Operation ended.
    /// </summary>
    /// <returns>
    /// A <see cref="WebhookDeliverySent"/>, <see cref="WebhookDeliveryFailed"/> or
    /// <see cref="WebhookDeliverySettled"/>; or an <see cref="UnknownWebhookDeliveryResult"/>
    /// for a kind this version of the library doesn't know.
    /// </returns>
    /// <exception cref="OperationFailedException">The Operation failed.</exception>
    /// <exception cref="InvalidOperationException">The Operation is not done, or is not a Test delivery or redrive.</exception>
    public WebhookDeliveryResult GetDeliveryResult()
    {
        if (SettledResponse() is not { ValueKind: JsonValueKind.Object } json
            || !json.TryGetProperty("_tag", out var tag) || tag.ValueKind != JsonValueKind.String)
        {
            throw NotProduced("a webhook delivery result");
        }

        try
        {
            return json.Deserialize(CoreJsonContext.Default.WebhookDeliveryResult)!;
        }
        catch (JsonException)
        {
            throw NotProduced("a webhook delivery result");
        }
    }

    // The ceiling doubles from the interval up to 10 s (an interval longer
    // than that stays as it is), and jitter picks 50-100 % of it so that many
    // waiters don't poll in step. Nothing waits less than the interval.
    private static TimeSpan PollDelay(TimeSpan interval, int poll)
    {
        var ceiling = Math.Min(interval.Ticks * Math.Pow(2, poll - 1), Math.Max(MaxInterval.Ticks, interval.Ticks));
        return TimeSpan.FromTicks(Math.Max(interval.Ticks, (long)(ceiling * (0.5 + (Jitter.Next() * 0.5)))));
    }

    private static OperationTimeoutException TimedOut(Operation latest, TimeSpan timeout) =>
        new($"Operation {latest.Name} was not done after {timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)} s.", latest);

    /// <summary>The <see cref="Response"/> of this done Operation, or the error it finished with.</summary>
    private JsonElement? SettledResponse() =>
        Done ? Settled().Response : throw new InvalidOperationException($"Operation {Name} is not done; call WaitAsync first.");

    private InvalidOperationException NotProduced(string what) => new($"Operation {Name} did not produce {what}.");

    /// <summary>This done Operation, or the error it finished with.</summary>
    private Operation Settled() => Error is null ? this : throw OperationFailedException.From(this, Error);
}
