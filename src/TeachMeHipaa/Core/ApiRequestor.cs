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
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.TryAddWithoutValidation("User-Agent", _config.UserAgent);
        // Fixed once per call, so every retry and the 401 replay send the same key.
        var idempotencyKey = requestOptions?.IdempotencyKey
            ?? (request.IdempotencyKeyRequired ? Guid.NewGuid().ToString() : null);
        if (idempotencyKey is not null)
        {
            message.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        if (requestOptions?.RequestId is { } requestId)
        {
            message.Headers.TryAddWithoutValidation("x-request-id", requestId);
        }

        if (request.Body is { } body)
        {
            message.Content = new ByteArrayContent(body.Content);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(body.ContentType);
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
