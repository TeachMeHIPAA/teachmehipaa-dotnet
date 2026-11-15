namespace TeachMeHipaa;

/// <summary>
/// The ETag a conditional request sends in <c>If-Match</c> or
/// <c>If-None-Match</c>: a tag you kept, such as
/// <see cref="TeachMeHipaaResponse.ETag"/>, or an object the API returned,
/// whose <see cref="TeachMeHipaaEntity.LastResponse"/> ETag is used.
/// </summary>
/// <remarks>
/// Convert from either implicitly: <c>new LearnerUpdateOptions(learner)</c>
/// or <c>new LearnerUpdateOptions("\"9f86d0…\"")</c>. An object whose response
/// carried no ETag is refused with an <see cref="ArgumentException"/> before
/// anything is sent.
/// </remarks>
public readonly struct ETag : IEquatable<ETag>
{
    private readonly string? _value;
    private readonly TeachMeHipaaEntity? _source;

    /// <summary>A tag as the API sent it, quotes included, or <c>*</c> for any current version.</summary>
    /// <param name="value">The tag.</param>
    public ETag(string value) => _value = value;

    private ETag(TeachMeHipaaEntity source) => _source = source;

    /// <summary>The ETag of the response <paramref name="source"/> came from.</summary>
    /// <param name="source">An object the API returned.</param>
    /// <returns>The object's ETag, read when the request is sent.</returns>
    public static ETag From(TeachMeHipaaEntity source) => new(source);

    /// <summary>A tag as the API sent it.</summary>
    /// <param name="value">The tag.</param>
    public static implicit operator ETag(string value) => new(value);

    /// <summary>The ETag of the response <paramref name="source"/> came from.</summary>
    /// <param name="source">An object the API returned.</param>
    public static implicit operator ETag(TeachMeHipaaEntity source) => new(source);

    /// <inheritdoc />
    public bool Equals(ETag other) => _value == other._value && ReferenceEquals(_source, other._source);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ETag other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _source is null ? _value?.GetHashCode() ?? 0 : _source.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => _value ?? _source?.LastResponse?.ETag ?? string.Empty;

    /// <summary>Compares two tags.</summary>
    /// <param name="left">A tag.</param>
    /// <param name="right">Another tag.</param>
    /// <returns>Whether they are the same tag or come from the same object.</returns>
    public static bool operator ==(ETag left, ETag right) => left.Equals(right);

    /// <summary>Compares two tags.</summary>
    /// <param name="left">A tag.</param>
    /// <param name="right">Another tag.</param>
    /// <returns>Whether they differ.</returns>
    public static bool operator !=(ETag left, ETag right) => !left.Equals(right);

    /// <summary>The header value, or an <see cref="ArgumentException"/> naming <paramref name="parameterName"/>.</summary>
    internal string Resolve(string header, string parameterName)
    {
        var value = _source is null ? _value : _source.LastResponse?.ETag;
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException(
                _source is null
                    ? $"{header} needs an ETag."
                    : $"{header} needs an ETag, but the {_source.GetType().Name} given came from a response without one.",
                parameterName);
        }

        return value!;
    }
}
