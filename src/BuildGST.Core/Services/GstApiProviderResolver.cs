using System;
using System.Collections.Generic;
using System.Linq;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Core.Services;

public sealed class GstApiProviderResolver : IGstApiProviderResolver
{
    private readonly IReadOnlyDictionary<string, IGstApiProvider> _providers;
    private readonly GstApiProviderOptions _options;

    public GstApiProviderResolver(IEnumerable<IGstApiProvider> providers, GstApiProviderOptions options)
    {
        if (providers == null)
        {
            throw new ArgumentNullException(nameof(providers));
        }

        _providers = providers.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IGstApiProvider Resolve(string? providerName = null)
    {
        var resolvedName = string.IsNullOrWhiteSpace(providerName) ? _options.DefaultProvider : providerName!;

        if (!_providers.TryGetValue(resolvedName, out var provider))
        {
            throw new InvalidOperationException($"No GST API provider is registered with the name '{resolvedName}'.");
        }

        return provider;
    }
}
