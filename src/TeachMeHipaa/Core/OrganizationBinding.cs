using System.Diagnostics.CodeAnalysis;

namespace TeachMeHipaa;

/// <summary>
/// The Organization a client acts for (ADR-0003): the configured one, or the
/// one its Access token is bound to, looked up once. Concurrent first calls
/// share one lookup, and a failed lookup is tried again on the next call.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "SemaphoreSlim without AvailableWaitHandle holds no unmanaged resources.")]
internal sealed class OrganizationBinding(string? organizationId, Func<CancellationToken, Task<string>> lookUp)
{
    private readonly SemaphoreSlim _lookUpLock = new(1, 1);
    private volatile string? _organizationId = organizationId;

    public async Task<string> GetOrganizationIdAsync(CancellationToken cancellationToken)
    {
        if (_organizationId is { } known)
        {
            return known;
        }

        await _lookUpLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return _organizationId ??= await lookUp(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lookUpLock.Release();
        }
    }
}
