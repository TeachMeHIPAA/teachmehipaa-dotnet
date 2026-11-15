using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>A token endpoint request, as every grant sends it.</summary>
internal static class TokenRequest
{
    /// <param name="tokenUri">The token endpoint, from <see cref="TokenEndpoint.UriFor"/>.</param>
    /// <param name="basicCredentials">The client's credentials, from <see cref="BasicCredentials.Encode"/>.</param>
    /// <param name="userAgent">The library's User-Agent, with the caller's AppInfo.</param>
    /// <param name="form">The grant's parameters.</param>
    /// <param name="scopes">The Scopes to ask for. With none, scope is left out and the endpoint grants all the credential holds.</param>
    public static HttpRequestMessage Create(
        Uri tokenUri,
        string basicCredentials,
        string userAgent,
        IEnumerable<KeyValuePair<string, string>> form,
        IReadOnlyCollection<string> scopes)
    {
        var parameters = scopes.Count > 0
            ? form.Append(new KeyValuePair<string, string>("scope", string.Join(" ", scopes)))
            : form;
        var request = new HttpRequestMessage(HttpMethod.Post, tokenUri)
        {
            Content = new FormUrlEncodedContent(parameters),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        return request;
    }
}
