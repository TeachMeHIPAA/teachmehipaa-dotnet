using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace TeachMeHipaa;

/// <summary>
/// Sends API requests through the client's handler pipeline: binds the
/// Organization, adds the standard headers and turns the response into a
/// model or a <see cref="TeachMeHipaaException"/>.
/// </summary>
internal sealed class ApiRequestor
{
    private readonly ClientConfiguration _config;
    private readonly HttpMessageInvoker _api;
    private readonly OrganizationBinding _organization;

    public ApiRequestor(ClientConfiguration config, HttpMessageInvoker api)
    {
        _config = config;
        _api = api;
        _organization = new OrganizationBinding(config.OrganizationId, LookUpOrganizationAsync);
    }

    /// <summary>The client's clock, for waits outside the handler pipeline.</summary>
    public TimeProvider TimeProvider => _config.TimeProvider;

    /// <summary>Sends a call whose success response is a JSON object.</summary>
    public async Task<T> RequestAsync<T>(
        ApiRequest request,
        RequestOptions? requestOptions,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
        where T : TeachMeHipaaEntity
    {
        using var response = await SendAsync(request, requestOptions, cancellationToken).ConfigureAwait(false);
        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        var value = response.IsSuccessStatusCode ? ResponseReader.Parse(response, text, typeInfo) : null;
        if (value is null)
        {
            throw ResponseReader.ApiError(response, text, _config.TimeProvider.GetUtcNow());
        }

        value.LastResponse = TeachMeHipaaResponse.From(response);
        return value;
    }

    /// <summary>
    /// Sends a list call. The page fetches the next one with the same
    /// request, only its position changed, so filters, limit and
    /// <paramref name="requestOptions"/> carry over.
    /// </summary>
    public async Task<Page<T>> RequestPageAsync<T>(
        ApiRequest request,
        RequestOptions? requestOptions,
        JsonTypeInfo<PageEnvelope<T>> typeInfo,
        PagePosition<T> position,
        CancellationToken cancellationToken)
    {
        var envelope = await RequestAsync(request, requestOptions, typeInfo, cancellationToken).ConfigureAwait(false);
        return new Page<T>
        {
            Items = envelope.Items,
            NextCursor = envelope.NextCursor,
            LastResponse = envelope.LastResponse,
            FetchNext = envelope.NextCursor is { } nextCursor
                ? token => RequestPageAsync(
                    request with { Query = WithParameter(request.Query, position.Parameter, position.NextPosition(envelope.Items, nextCursor)) },
                    requestOptions,
                    typeInfo,
                    position,
                    token)
                : null,
        };
    }

    /// <summary>
    /// Sends a call whose IR response is a <c>writeResult</c>: the body is the
    /// same whether the call created the object or found it, so
    /// <paramref name="withCreated"/> records which from the status (201 or 200).
    /// </summary>
    public async Task<T> RequestWriteResultAsync<T>(
        ApiRequest request,
        RequestOptions? requestOptions,
        JsonTypeInfo<T> typeInfo,
        Func<T, bool, T> withCreated,
        CancellationToken cancellationToken)
        where T : TeachMeHipaaEntity
    {
        var result = await RequestAsync(request, requestOptions, typeInfo, cancellationToken).ConfigureAwait(false);
        return withCreated(result, result.LastResponse?.StatusCode == HttpStatusCode.Created);
    }

    /// <summary>Sends a call whose success response is a file of the request's <see cref="ApiRequest.Accept"/> type.</summary>
    public async Task<BinaryResponse> RequestBinaryAsync(
        ApiRequest request,
        RequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(request, requestOptions, cancellationToken).ConfigureAwait(false);
        var contentType = response.Content?.Headers.ContentType?.MediaType;
        // Unknown paths answer with a 200 HTML page, so only the promised type counts as the file.
        if (response.IsSuccessStatusCode && string.Equals(contentType, request.Accept, StringComparison.OrdinalIgnoreCase))
        {
            return new BinaryResponse
            {
                Data = await ResponseReader.ReadBytesAsync(response, cancellationToken).ConfigureAwait(false),
                ContentType = contentType,
                FileName = ResponseReader.FileName(response),
                LastResponse = TeachMeHipaaResponse.From(response),
            };
        }

        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        throw ResponseReader.ApiError(response, text, _config.TimeProvider.GetUtcNow());
    }

    /// <summary>Sends a call whose success response has no body, such as a 204 from a DELETE.</summary>
    public async Task<TeachMeHipaaResponse> RequestAsync(
        ApiRequest request,
        RequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(request, requestOptions, cancellationToken).ConfigureAwait(false);
        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        // A body is ignored only when it is JSON: an HTML page still means a wrong base URL.
        if (!response.IsSuccessStatusCode || (text.Length > 0 && !ResponseReader.IsJson(response)))
        {
            throw ResponseReader.ApiError(response, text, _config.TimeProvider.GetUtcNow());
        }

        return TeachMeHipaaResponse.From(response);
    }

    private async Task<HttpResponseMessage> SendAsync(
        ApiRequest request,
        RequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        var path = request.OrganizationScoped
            ? $"/organizations/{Uri.EscapeDataString(await _organization.GetOrganizationIdAsync(cancellationToken).ConfigureAwait(false))}{request.Path}"
            : request.Path;
        using var message = new HttpRequestMessage(request.Method, new Uri($"{_config.BaseUrl}{path}{QueryString(request.Query)}"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(request.Accept ?? "application/json"));
        message.Headers.TryAddWithoutValidation("User-Agent", _config.UserAgent);
        // Fixed once per call, so every retry and the 401 replay send the same key.
        var idempotencyKey = requestOptions?.IdempotencyKey
            ?? (request.IdempotencyKeyRequired ? Guid.NewGuid().ToString() : null);
        if (idempotencyKey is not null)
        {
            message.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        if (requestOptions?.IfMatch is { } ifMatch)
        {
            message.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        if (requestOptions?.RequestId is { } requestId)
        {
            message.Headers.TryAddWithoutValidation("x-request-id", requestId);
        }

        if (request.Body is { } body)
        {
            message.Content = body.CreateContent();
        }

        if (requestOptions?.Timeout is { } timeout)
        {
            RequestProperties.Timeout.Set(message, timeout);
        }

        if (requestOptions?.MaxRetries is { } maxRetries)
        {
            RequestProperties.MaxRetries.Set(message, maxRetries);
        }

        RequestProperties.RetrySafe.Set(message, request.RetrySafe);
        RequestProperties.Authenticated.Set(message, request.Authenticated);
        return await _api.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> LookUpOrganizationAsync(CancellationToken cancellationToken)
    {
        var introspection = await RequestAsync(
            new ApiRequest(HttpMethod.Get, "/auth/introspection") { RetrySafe = true },
            requestOptions: null,
            CoreJsonContext.Default.OrganizationIntrospection,
            cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(introspection.OrgId))
        {
            return introspection.OrgId!;
        }

        throw new TeachMeHipaaException(
            "The Access token is not bound to an Organization. Set TeachMeHipaaClientOptions.OrganizationId.")
        {
            StatusCode = introspection.LastResponse?.StatusCode,
            RequestId = introspection.LastResponse?.RequestId,
            Headers = introspection.LastResponse?.Headers ?? new Dictionary<string, string>(),
        };
    }

    private static KeyValuePair<string, string?>[] WithParameter(
        IEnumerable<KeyValuePair<string, string?>>? query,
        string name,
        string value) =>
        [.. (query ?? []).Select(parameter => parameter.Key == name ? new(name, value) : parameter)];

    private static string QueryString(IEnumerable<KeyValuePair<string, string?>>? query)
    {
        var builder = new StringBuilder();
        foreach (var parameter in query ?? [])
        {
            if (parameter.Value is not null)
            {
                builder.Append(builder.Length == 0 ? '?' : '&')
                    .Append(Uri.EscapeDataString(parameter.Key))
                    .Append('=')
                    .Append(Uri.EscapeDataString(parameter.Value));
            }
        }

        return builder.ToString();
    }
}
