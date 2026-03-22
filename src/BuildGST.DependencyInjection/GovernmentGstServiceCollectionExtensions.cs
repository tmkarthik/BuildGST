using System;
using System.Net.Http;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Http.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildGST.DependencyInjection;

/// <summary>
/// Adds registration helpers for the government GST provider.
/// </summary>
public static class GovernmentGstServiceCollectionExtensions
{
    /// <summary>
    /// Registers the government GST provider in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The provider options configuration.</param>
    /// <param name="configureRetry">The retry options configuration.</param>
    /// <param name="httpClientFactory">An optional custom HTTP client factory.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGovernmentGstProvider(
        this IServiceCollection services,
        Action<GstApiProviderOptions> configureOptions,
        Action<GstApiRetryOptions>? configureRetry = null,
        Func<IServiceProvider, HttpClient>? httpClientFactory = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        var options = new GstApiProviderOptions();
        configureOptions(options);

        var retryOptions = new GstApiRetryOptions();
        configureRetry?.Invoke(retryOptions);

        services.AddSingleton(options);
        services.AddSingleton<IOptions<GstApiProviderOptions>>(Options.Create(options));
        services.AddSingleton(retryOptions);
        services.AddSingleton<GovernmentGstProvider>(serviceProvider =>
        {
            var httpClient = httpClientFactory?.Invoke(serviceProvider) ?? new HttpClient();
            return new GovernmentGstProvider(
                httpClient,
                serviceProvider.GetRequiredService<GstApiProviderOptions>(),
                serviceProvider.GetRequiredService<GstApiRetryOptions>());
        });
        services.AddSingleton<IGstApiProvider>(serviceProvider => serviceProvider.GetRequiredService<GovernmentGstProvider>());

        return services;
    }
}
