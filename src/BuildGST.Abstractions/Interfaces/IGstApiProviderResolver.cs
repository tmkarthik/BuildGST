namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Resolves the active GST API provider strategy based on configuration.
/// </summary>
public interface IGstApiProviderResolver
{
    /// <summary>
    /// Resolves the configured GST API provider.
    /// </summary>
    /// <returns>The configured provider implementation.</returns>
    IGstApiProvider Resolve();
}
