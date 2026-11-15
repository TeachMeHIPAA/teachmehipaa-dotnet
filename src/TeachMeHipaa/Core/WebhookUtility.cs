using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TeachMeHipaa;

/// <summary>
/// Verifies Deliveries and signs payloads for your own tests. Needs only the
/// Signing secret, never API credentials. Also reachable as
/// <see cref="TeachMeHipaaClient.Webhooks"/>.
/// </summary>
public static class WebhookUtility
{
    private const string SignatureHeader = "x-tmh-signature";
    private const string TimestampHeader = "x-tmh-timestamp";
    private const string BodyHashHeader = "x-tmh-body-hash";

    private const string RawBodyHint =
        "Are you passing the raw request body exactly as received? Parsed and re-serialized JSON does not verify.";

    private const string HexDigits = "0123456789abcdef";

    // DateTimeOffset.Parse also takes a date-time without an offset, read as
    // local time, which would silently shift the tolerance window. ISO 8601
    // allows any number of fractional digits; DateTimeOffset holds 7.
    private static readonly Regex Iso8601DateTime = new(
        @"\A([0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2})(?:\.([0-9]{1,7})[0-9]*)?(Z|[+-][0-9]{2}:[0-9]{2})\z",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// How far a Delivery's timestamp may be from when it arrived, in either
    /// direction, unless you pass another tolerance: 5 minutes.
    /// </summary>
    public static TimeSpan DefaultTolerance { get; } = TimeSpan.FromMinutes(5);

    /// <summary>Verifies a Delivery and returns its Event, or the Test delivery.</summary>
    /// <param name="payload">The raw request body exactly as received. Parsed and re-serialized JSON does not verify.</param>
    /// <param name="headers">The request headers, such as a dictionary, matched without regard to case.</param>
    /// <param name="secret">The Webhook subscription's Signing secret.</param>
    /// <param name="path">
    /// The path, with its query string, the Delivery was sent to (such as
    /// <c>/webhooks/tmh?x=1</c>), or the full request URL. The signature covers it.
    /// </param>
    /// <param name="tolerance">How far the Delivery's timestamp may be from <paramref name="receivedAt"/>, in either direction. Defaults to <see cref="DefaultTolerance"/>.</param>
    /// <param name="receivedAt">When the Delivery arrived. Defaults to now.</param>
    /// <returns>The verified Delivery: an Event, or a Test delivery.</returns>
    /// <exception cref="WebhookSignatureException">The Delivery failed verification.</exception>
    /// <exception cref="ArgumentException">The secret is empty, the path is not a path or URL, or the tolerance is not positive.</exception>
    public static WebhookDelivery ConstructEvent(
        string payload,
        IEnumerable<KeyValuePair<string, string>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(Utf8(payload), Lookup(headers), secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="ConstructEvent(string, IEnumerable{KeyValuePair{string, string}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    /// <remarks>
    /// Takes multi-valued request headers, such as <see cref="System.Net.Http.Headers.HttpHeaders"/>
    /// or an Azure Functions <c>HttpRequestData.Headers</c>, and reads the first value of each.
    /// </remarks>
    public static WebhookDelivery ConstructEvent(
        string payload,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(Utf8(payload), Lookup(headers), secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="ConstructEvent(string, IEnumerable{KeyValuePair{string, string}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    /// <remarks>
    /// <c>getHeader</c> returns a request header's value, or null when it is
    /// absent. It is called with lowercase names, so look them up without
    /// regard to case, as ASP.NET Core does: <c>name => Request.Headers[name]</c>.
    /// </remarks>
    public static WebhookDelivery ConstructEvent(
        string payload,
        Func<string, string?> getHeader,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(Utf8(payload), getHeader ?? throw new ArgumentNullException(nameof(getHeader)), secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="ConstructEvent(string, IEnumerable{KeyValuePair{string, string}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public static WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        IEnumerable<KeyValuePair<string, string>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(payload.ToArray(), Lookup(headers), secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="ConstructEvent(string, IEnumerable{KeyValuePair{string, IEnumerable{string}}}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public static WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(payload.ToArray(), Lookup(headers), secret, path, tolerance, receivedAt);

    /// <inheritdoc cref="ConstructEvent(string, Func{string, string}, string, string, TimeSpan?, DateTimeOffset?)"/>
    public static WebhookDelivery ConstructEvent(
        ReadOnlySpan<byte> payload,
        Func<string, string?> getHeader,
        string secret,
        string path,
        TimeSpan? tolerance = null,
        DateTimeOffset? receivedAt = null) =>
        Verify(payload.ToArray(), getHeader ?? throw new ArgumentNullException(nameof(getHeader)), secret, path, tolerance, receivedAt);

    /// <summary>
    /// Returns the headers TeachMeHIPAA would send with <paramref name="payload"/>
    /// to <paramref name="path"/>, so you can test your handler without a real Delivery.
    /// </summary>
    /// <param name="payload">The exact body you will send.</param>
    /// <param name="secret">The Signing secret your handler verifies with.</param>
    /// <param name="path">The path, with its query string, your handler is mounted on, or the full URL.</param>
    /// <param name="timestamp">When the Delivery was signed. Defaults to now.</param>
    /// <param name="eventType">The <c>x-tmh-event-type</c> header; left out when null.</param>
    /// <param name="eventId">The <c>x-tmh-event-id</c> header; left out when null.</param>
    /// <param name="keyId">The <c>x-tmh-key-id</c> header; left out when null.</param>
    /// <returns>The headers, looked up without regard to case.</returns>
    /// <exception cref="ArgumentException">The secret is empty or the path is not a path or URL.</exception>
    public static IReadOnlyDictionary<string, string> GenerateTestHeaders(
        string payload,
        string secret,
        string path,
        DateTimeOffset? timestamp = null,
        string? eventType = null,
        string? eventId = null,
        string? keyId = null) =>
        Headers(Utf8(payload), secret, path, timestamp, eventType, eventId, keyId);

    /// <inheritdoc cref="GenerateTestHeaders(string, string, string, DateTimeOffset?, string, string, string)"/>
    public static IReadOnlyDictionary<string, string> GenerateTestHeaders(
        ReadOnlySpan<byte> payload,
        string secret,
        string path,
        DateTimeOffset? timestamp = null,
        string? eventType = null,
        string? eventId = null,
        string? keyId = null) =>
        Headers(payload.ToArray(), secret, path, timestamp, eventType, eventId, keyId);

    private static Dictionary<string, string> Headers(
        byte[] body,
        string secret,
        string path,
        DateTimeOffset? timestamp,
        string? eventType,
        string? eventId,
        string? keyId)
    {
        CheckSecret(secret);
        var signedPath = PathAndQuery(path);
        // The platform's format: JavaScript's `Date.toISOString()`, always UTC
        // with milliseconds.
        var signedAt = (timestamp ?? DateTimeOffset.UtcNow)
            .ToUniversalTime()
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
        var bodyHash = Sha256Hex(body);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (keyId is not null)
        {
            headers["x-tmh-key-id"] = keyId;
        }

        headers[TimestampHeader] = signedAt;
        headers[SignatureHeader] = Sign(secret, signedPath, signedAt, bodyHash);
        headers[BodyHashHeader] = bodyHash;
        if (eventType is not null)
        {
            headers["x-tmh-event-type"] = eventType;
        }

        if (eventId is not null)
        {
            headers["x-tmh-event-id"] = eventId;
        }

        headers["content-type"] = "application/json";
        return headers;
    }

    private static WebhookDelivery Verify(
        byte[] body,
        Func<string, string?> header,
        string secret,
        string path,
        TimeSpan? tolerance,
        DateTimeOffset? receivedAt)
    {
        CheckSecret(secret);
        var signedPath = PathAndQuery(path);
        var window = tolerance ?? DefaultTolerance;
        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "tolerance must be positive.");
        }

        var signature = RequiredHeader(header, SignatureHeader);
        var timestamp = RequiredHeader(header, TimestampHeader);
        var signedAt = ParseTimestamp(timestamp);
        var bodyHash = Sha256Hex(body);

        var sentBodyHash = header(BodyHashHeader);
        if (sentBodyHash is not null && !string.Equals(sentBodyHash, bodyHash, StringComparison.Ordinal))
        {
            throw Rejected(
                WebhookSignatureException.BodyHashMismatch,
                $"The {BodyHashHeader} header does not match the body. {RawBodyHint}");
        }

        var expected = Sign(secret, signedPath, timestamp, bodyHash);
        if (!IsHexSha256(signature) || !FixedTimeEquals(expected, signature))
        {
            throw Rejected(
                WebhookSignatureException.SignatureMismatch,
                "No signature matched the expected signature for this Delivery. " +
                $"{RawBodyHint} Are you passing the exact request path, with its query string, that the " +
                "Delivery was sent to, and the Webhook subscription's Signing secret?");
        }

        // Only after the signature, so a forgery is never reported as merely late.
        if (((receivedAt ?? DateTimeOffset.UtcNow) - signedAt).Duration() > window)
        {
            throw Rejected(
                WebhookSignatureException.TimestampOutsideTolerance,
                FormattableString.Invariant(
                    $"The Delivery's timestamp {timestamp} is more than {window.TotalSeconds} seconds from when it arrived. ") +
                "Check your server's clock; otherwise the Delivery is being replayed.");
        }

        return Parse(body);
    }

    private static Func<string, string?> Lookup(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _ = headers ?? throw new ArgumentNullException(nameof(headers));
        return name => headers.FirstOrDefault(h => string.Equals(h.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static Func<string, string?> Lookup(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        _ = headers ?? throw new ArgumentNullException(nameof(headers));
        return name => headers
            .FirstOrDefault(h => string.Equals(h.Key, name, StringComparison.OrdinalIgnoreCase))
            .Value?.FirstOrDefault();
    }

    private static string RequiredHeader(Func<string, string?> header, string name)
    {
        var value = header(name);
        if (value is null || value.Length == 0)
        {
            throw Rejected(
                WebhookSignatureException.MissingHeader,
                $"No {name} header found on the Delivery. Are you passing the request headers as TeachMeHIPAA sent them?");
        }

        return value;
    }

    private static DateTimeOffset ParseTimestamp(string timestamp)
    {
        var match = Iso8601DateTime.Match(timestamp);
        if (match.Success && DateTimeOffset.TryParseExact(
                match.Groups[1].Value + "." + match.Groups[2].Value.PadRight(7, '0') + match.Groups[3].Value,
                "yyyy-MM-dd'T'HH:mm:ss.fffffffK",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var signedAt))
        {
            return signedAt;
        }

        throw Rejected(
            WebhookSignatureException.InvalidTimestamp,
            $"The {TimestampHeader} header \"{timestamp}\" is not an ISO 8601 date-time with an offset.");
    }

    // Mirrors the platform signer, which signs the endpoint URL's path and
    // query and takes a "/"-prefixed path as given.
    private static string PathAndQuery(string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));
        if (path.Length > 0 && path[0] == '/')
        {
            return path;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var url) && (url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeHttp))
        {
            return url.PathAndQuery;
        }

        throw new ArgumentException(
            $"path must be the request path (such as \"/webhooks/tmh?x=1\") or the full request URL, got \"{path}\".",
            nameof(path));
    }

    private static void CheckSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException(
                "secret must be the Webhook subscription's Signing secret; got an empty value.",
                nameof(secret));
        }
    }

    // ADR-0005; the vendored IR's `events.signing.canonical` describes the same
    // four parts, and a test pins the two together.
    private static string Sign(string secret, string signedPath, string timestamp, string bodyHash)
    {
        // The key is the Signing secret's UTF-8 bytes. The secret looks like
        // base64, but the platform never decodes it.
        var key = Encoding.UTF8.GetBytes(secret);
        var canonical = Encoding.UTF8.GetBytes($"POST\n{signedPath}\n{timestamp}\n{bodyHash}");
#if NET5_0_OR_GREATER
        return Hex(HMACSHA256.HashData(key, canonical));
#else
        using var hmac = new HMACSHA256(key);
        return Hex(hmac.ComputeHash(canonical));
#endif
    }

    private static string Sha256Hex(byte[] body)
    {
#if NET5_0_OR_GREATER
        return Hex(SHA256.HashData(body));
#else
        using var sha256 = SHA256.Create();
        return Hex(sha256.ComputeHash(body));
#endif
    }

    private static string Hex(byte[] bytes)
    {
        var digits = new char[bytes.Length * 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            digits[2 * i] = HexDigits[bytes[i] >> 4];
            digits[(2 * i) + 1] = HexDigits[bytes[i] & 0xF];
        }

        return new string(digits);
    }

    // The platform sends lowercase hex, so anything else is a mismatch, and
    // past this check both sides are 64 ASCII characters.
    private static bool IsHexSha256(string signature) =>
        signature.Length == 64 && signature.All(c => c is (>= '0' and <= '9') or (>= 'a' and <= 'f'));

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static bool FixedTimeEquals(string expected, string actual)
    {
#if NET5_0_OR_GREATER
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(actual));
#else
        // No early exit, so the time taken never reveals how much of the
        // signature matched.
        var difference = 0;
        for (var i = 0; i < expected.Length; i++)
        {
            difference |= expected[i] ^ actual[i];
        }

        return difference == 0;
#endif
    }

    private static WebhookSignatureException Rejected(string code, string message) => new(message) { Code = code };

    private static byte[] Utf8(string payload) =>
        Encoding.UTF8.GetBytes(payload ?? throw new ArgumentNullException(nameof(payload)));

    // A Test delivery is not an Event envelope (ADR-0005), so it never reaches
    // the Event converter, whose fallback would misread it as an UnknownEvent.
    private static WebhookDelivery Parse(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        var json = document.RootElement;
        if (json.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The Delivery body is not a JSON object.");
        }

        if (json.TryGetProperty("test", out var test) && test.ValueKind == JsonValueKind.True
            && json.TryGetProperty("subscriptionId", out _))
        {
            return new(json.Deserialize(TeachMeHipaaJsonContext.Default.TestDelivery)!);
        }

        return new(json.Deserialize(TeachMeHipaaJsonContext.Default.Event)!);
    }
}
