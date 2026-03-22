namespace BuildGST.Abstractions.Models;

/// <summary>
/// Defines the supported GST API provider strategies.
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// Government-hosted provider.
    /// </summary>
    Government = 1,

    /// <summary>
    /// Third-party commercial provider.
    /// </summary>
    ThirdParty = 2,

    /// <summary>
    /// Sandbox or mock provider for testing and non-production use.
    /// </summary>
    Sandbox = 3
}
