using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TeachMeHipaa;

/// <summary>
/// A field of a merge-patch update that tells "leave it as it is" from "set
/// it", null included. Left unset (the default), the field is left out of
/// the request and keeps its value; set, it is sent, and a null clears it.
/// </summary>
/// <remarks>
/// Assign the value directly: <c>new LearnerUpdateOptions(learner) { ExternalId = null }</c>
/// clears the external id and leaves the name alone. A value typed as an
/// interface, such as an <c>IReadOnlyList&lt;T&gt;</c> variable, needs
/// <c>new Optional&lt;T&gt;(value)</c>, as C# has no implicit conversion from one.
/// </remarks>
/// <typeparam name="T">The field's type.</typeparam>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The design grilling (Q28) names it Optional<T>; VB callers write [Optional].")]
public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    /// <summary>A field set to <paramref name="value"/>.</summary>
    /// <param name="value">The value to send; null clears the field.</param>
    public Optional(T value)
    {
        Value = value;
        IsSet = true;
    }

    /// <summary>Whether the field is sent.</summary>
    public bool IsSet { get; }

    /// <summary>The value sent, or the type's default when <see cref="IsSet"/> is false.</summary>
    public T Value { get; }

    /// <summary>A field set to <paramref name="value"/>.</summary>
    /// <param name="value">The value to send; null clears the field.</param>
    public static implicit operator Optional<T>(T value) => new(value);

    /// <inheritdoc />
    public bool Equals(Optional<T> other) =>
        IsSet == other.IsSet && EqualityComparer<T>.Default.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => IsSet ? EqualityComparer<T>.Default.GetHashCode(Value!) : 0;

    /// <inheritdoc />
    public override string ToString() => IsSet ? Value?.ToString() ?? "null" : "(unset)";

    /// <summary>Compares two fields.</summary>
    /// <param name="left">A field.</param>
    /// <param name="right">Another field.</param>
    /// <returns>Whether both are unset, or both set to equal values.</returns>
    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    /// <summary>Compares two fields.</summary>
    /// <param name="left">A field.</param>
    /// <param name="right">Another field.</param>
    /// <returns>Whether they differ.</returns>
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}

/// <summary>
/// Writes an <see cref="Optional{T}"/> as its value, null included. The
/// property's <c>WhenWritingDefault</c> leaves an unset one out, since unset
/// is the struct's default.
/// </summary>
/// <remarks>
/// Each generated property names its closed converter, because a converter
/// factory would need runtime generics that trimming and AOT forbid.
/// </remarks>
internal sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(JsonSerializer.Deserialize(ref reader, TypeInfo(options))!);

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.Value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, TypeInfo(options));
    }

    private static JsonTypeInfo<T> TypeInfo(JsonSerializerOptions options) => (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
}
