using System.Diagnostics.CodeAnalysis;

namespace TeachMeHipaa;

/// <summary>
/// What a <c>GetIfModifiedAsync</c> call found: the object when it changed
/// since the ETag you sent, or nothing when your copy is still current (304).
/// </summary>
/// <remarks>Keep using your copy when nothing came back: <c>var current = result.Value ?? cached;</c></remarks>
/// <typeparam name="T">The object retrieved.</typeparam>
public sealed class ConditionalResult<T>
    where T : TeachMeHipaaEntity
{
    internal ConditionalResult(T? value, TeachMeHipaaResponse lastResponse)
    {
        Value = value;
        LastResponse = lastResponse;
    }

    /// <summary>True when the API answered 304: your copy is current and <see cref="Value"/> is null.</summary>
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsNotModified => Value is null;

    /// <summary>The object as it is now, or null when it hasn't changed.</summary>
    public T? Value { get; }

    /// <summary>What the API said about the response, the 304 included: status, headers, request id and ETag.</summary>
    public TeachMeHipaaResponse LastResponse { get; }
}
