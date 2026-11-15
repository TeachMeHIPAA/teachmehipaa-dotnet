namespace TeachMeHipaa;

/// <summary>Identifies your software to TeachMeHIPAA in the User-Agent.</summary>
/// <param name="Name">Your software's name.</param>
/// <param name="Version">Your software's version.</param>
/// <param name="Url">Where to learn about your software.</param>
public sealed record AppInfo(string Name, string? Version = null, string? Url = null);
