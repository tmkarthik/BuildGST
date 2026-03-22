using System;
using System.Collections.Generic;
using System.Linq;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using Microsoft.Extensions.Options;

namespace BuildGST.Core.Services;

/// <summary>
/// Resolves a GST API provider strategy from the current configuration.
/// </summary>
public sealed class GstApiProviderResolver : IGstApiProviderResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<GstApiProviderOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GstApiProviderResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="options">The provider selection options.</param>
    public GstApiProviderResolver(IServiceProvider serviceProvider, IOptions<GstApiProviderOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public IGstApiProvider Resolve()
    {
        var providers = _serviceProvider.GetService(typeof(IEnumerable<IGstApiProvider>)) as IEnumerable<IGstApiProvider>;
        var provider = TryResolveProvider(_options.Value.Provider, providers);
        if (provider == null)
        {
            throw new InvalidOperationException($"No GST API provider is registered for '{_options.Value.Provider}'.");
        }

        return provider;
    }

    private static IGstApiProvider? TryResolveProvider(ProviderType providerType, IEnumerable<IGstApiProvider>? providers)
    {
        if (providers == null)
        {
            return null;
        }

        var providerName = GetProviderName(providerType);
        return providers.FirstOrDefault(provider => string.Equals(provider.Name, providerName, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetProviderName(ProviderType providerType)
    {
        switch (providerType)
        {
            case ProviderType.Government:
                return "government";
            case ProviderType.ThirdParty:
                return "third-party";
            case ProviderType.Sandbox:
                return "sandbox";
            default:
                return null;
        }
    }
}
