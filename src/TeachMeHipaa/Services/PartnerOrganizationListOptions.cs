// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

/// <summary>Parameters for <see cref="PartnerOrganizationService.ListAsync"/>.</summary>
public sealed class PartnerOrganizationListOptions
{
    /// <summary>How many objects to return, between 1 and 100. Defaults to 20.</summary>
    public long? Limit { get; set; }

    /// <summary>The <c>nextCursor</c> from the previous page.</summary>
    public string? Cursor { get; set; }
}
