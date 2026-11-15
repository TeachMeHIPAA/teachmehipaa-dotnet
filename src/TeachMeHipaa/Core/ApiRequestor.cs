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

    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path below the base URL, with path parameters already escaped.</param>
    /// <param name="organizationScoped">Whether to prefix the path with <c>/organizations/{orgId}</c>.</param>
    /// <param name="query">Query parameters; null values are left out.</param>
    /// <param name="requestOptions">The caller's per-call settings.</param>
    /// <param name="typeInfo">How to read the response body.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    public async Task<T> RequestAsync<T>(
        HttpMethod method,
        string path,
        bool organizationScoped,
        IEnumerable<KeyValuePair<string, string?>>? query,
        RequestOptions? requestOptions,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
        where T : TeachMeHipaaEntity
    {
        var fullPath = organizationScoped
            ? $"/organizations/{Uri.EscapeDataString(await _organization.GetOrganizationIdAsync(cancellationToken).ConfigureAwait(false))}{path}"
            : path;
        using var request = new HttpRequestMessage(method, new Uri($"{_config.BaseUrl}{fullPath}{QueryString(query)}"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", _config.UserAgent);
        if (requestOptions?.RequestId is { } requestId)
        {
            request.Headers.TryAddWithoutValidation("x-request-id", requestId);
        }

        if (requestOptions?.Timeout is { } timeout)
        {
            RequestTimeout.Set(request, timeout);
        }

        using var response = await _api.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        var value = response.IsSuccessStatusCode ? ResponseReader.Parse(response, text, typeInfo) : null;
        if (value is null)
        {
            throw ResponseReader.ApiError(response, text);
        }

        value.LastResponse = TeachMeHipaaResponse.From(response);
        return value;
    }

    private async Task<string> LookUpOrganizationAsync(CancellationToken cancellationToken)
    {
        var introspection = await RequestAsync(
            HttpMethod.Get,
            "/auth/introspection",
            organizationScoped: false,
            query: null,
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
