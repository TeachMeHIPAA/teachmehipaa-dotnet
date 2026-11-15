namespace TeachMeHipaa;

/// <summary>Identifies your integration to TeachMeHIPAA in the User-Agent.</summary>
/// <param name="Name">Your integration's name.</param>
/// <param name="Version">Your integration's version.</param>
/// <param name="Url">Where to learn about your integration.</param>
public sealed record AppInfo(string Name, string? Version = null, string? Url = null);
