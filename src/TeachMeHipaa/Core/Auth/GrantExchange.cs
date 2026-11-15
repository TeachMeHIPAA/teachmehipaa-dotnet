using System.Net.Http.Headers;

namespace TeachMeHipaa;

/// <summary>What to tell the caller when an exchange fails in a way only they can fix.</summary>
/// <param name="NetworkFailure">For a network failure, whose outcome is unknown.</param>
/// <param name="InvalidGrant">For <c>invalid_grant</c>, or null to keep the endpoint's words.</param>
internal sealed record ExchangeGuidance(string NetworkFailure, string? InvalidGrant = null);

/// <summary>
/// One <c>authorization_code</c> or <c>refresh_token</c> exchange, sent
/// exactly once (ADR-0004); a failure is raised with guidance instead.
/// </summary>
internal sealed record GrantExchange(GrantTokens Tokens, DateTimeOffset RequestedAt)
{
    private static readonly char[] ScopeSeparator = [' '];

    /// <summary>Counted from when the request left, so a slow response can't stretch the token past its real lifetime.</summary>
    public DateTimeOffset ExpiresAt => RequestedAt + Tokens.ExpiresIn;

    public static async Task<GrantExchange> SendAsync(
        TokenEndpoint endpoint,
        string basicCredentials,
        IEnumerable<KeyValuePair<string, string>> form,
        ExchangeGuidance guidance,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Uri)
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", endpoint.UserAgent);

        var requestedAt = endpoint.TimeProvider.GetUtcNow();
        HttpResponseMessage response;
        try
        {
            response = await endpoint.Invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiTimeoutException exception)
        {
            throw new ApiTimeoutException(
                ResponseReader.WithGuidance(exception.Message, guidance.NetworkFailure),
                exception.InnerException ?? exception);
        }
        catch (ApiConnectionException exception)
        {
            throw new ApiConnectionException(
                ResponseReader.WithGuidance(exception.Message, guidance.NetworkFailure),
                exception.InnerException ?? exception);
        }

        using (response)
        {
            var text = await ResponseReader.ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
            var now = endpoint.TimeProvider.GetUtcNow();
            if (!response.IsSuccessStatusCode)
            {
                throw ResponseReader.OAuthError(response, text, now, guidance.InvalidGrant);
            }

            if (!ResponseReader.IsJson(response))
            {
                throw ResponseReader.ApiError(response, text, now);
            }

            var token = ResponseReader.Parse(response, text, CoreJsonContext.Default.TokenEndpointResponse);
            if (token is not { AccessToken.Length: > 0, RefreshToken.Length: > 0, ExpiresIn: > 0 })
            {
                // No body or headers: the body may hold a live token, which
                // must not reach error reporters.
                throw new ApiException("The token endpoint answered without an access_token, refresh_token and expires_in.")
                {
                    StatusCode = response.StatusCode,
                };
            }

            return new GrantExchange(
                new GrantTokens
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = TimeSpan.FromSeconds(token.ExpiresIn.Value),
                    Scopes = token.Scope?.Split(ScopeSeparator, StringSplitOptions.RemoveEmptyEntries) ?? [],
                },
                requestedAt);
        }
    }
}
