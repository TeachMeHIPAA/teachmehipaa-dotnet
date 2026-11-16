namespace TeachMeHipaa;

/// <summary>A file the API returned, such as a Certificate PDF.</summary>
public sealed record BinaryResponse : TeachMeHipaaEntity
{
    /// <summary>The file's bytes, unchanged.</summary>
    public byte[] Data { get; init; } = [];

    /// <summary>The file's media type, such as <c>application/pdf</c>.</summary>
    public string? ContentType { get; init; }

    /// <summary>The file name the API suggested in <c>Content-Disposition</c>, if any.</summary>
    public string? FileName { get; init; }
}
