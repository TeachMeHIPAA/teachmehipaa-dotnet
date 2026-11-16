using System.Text.Json.Serialization;

namespace TeachMeHipaa;

/// <summary>The base of every object the API returns.</summary>
public abstract record TeachMeHipaaEntity
{
    /// <summary>
    /// The response this object came from: status, headers, request id and
    /// ETag. Null for an object you built or deserialized yourself.
    /// </summary>
    [JsonIgnore]
    public TeachMeHipaaResponse? LastResponse { get; internal set; }
}
