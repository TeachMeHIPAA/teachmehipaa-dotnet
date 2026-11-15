namespace TeachMeHipaa;

/// <summary>
/// Which TeachMeHIPAA deployment a credential belongs to. Credentials never
/// work across Environments.
/// </summary>
public enum TeachMeHipaaEnvironment
{
    /// <summary>The production deployment, <c>https://api.tme.so/v1</c>.</summary>
    Live,

    /// <summary>The test deployment, <c>https://api-teachmehipaa-dev.web.app/v1</c>.</summary>
    Sandbox,
}
