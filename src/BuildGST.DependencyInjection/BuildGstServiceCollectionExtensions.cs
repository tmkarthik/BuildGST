using System;
using System.Collections.Generic;
using System.Net.Http;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using BuildGST.Http.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildGST.DependencyInjection;

/// <summary>
/// Provides dependency injection registration helpers for the BuildGST library.
/// </summary>
public static class BuildGstServiceCollectionExtensions
{
    /// <summary>
    /// Registers the BuildGST services, validators, generators, resolver, and available providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The GST provider configuration callback.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBuildGst(
        this IServiceCollection services,
        Action<GstApiProviderOptions>? configureOptions = null)
    {
        return AddBuildGst(services, configureOptions, null);
    }

    /// <summary>
    /// Registers the BuildGST services, validators, generators, resolver, and available providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The GST provider configuration callback.</param>
    /// <param name="httpClientFactory">An optional HTTP client factory used for HTTP-based providers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBuildGst(
        this IServiceCollection services,
        Action<GstApiProviderOptions>? configureOptions = null,
        Func<IServiceProvider, HttpClient>? httpClientFactory = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new GstApiProviderOptions();
        configureOptions?.Invoke(options);
        var retryOptions = new GstApiRetryOptions();
        var sandboxTaxPayers = new Dictionary<string, GstTaxPayer>(StringComparer.OrdinalIgnoreCase);

        services.AddSingleton(options);
        services.AddSingleton<IOptions<GstApiProviderOptions>>(Options.Create(options));
        services.AddSingleton(retryOptions);
        services.AddSingleton<IReadOnlyDictionary<string, GstTaxPayer>>(sandboxTaxPayers);
        services.AddSingleton<IGstinValidator, GstinValidator>();
        services.AddSingleton<IInvoiceSchemaValidator, InvoiceSchemaValidator>();
        services.AddSingleton<IGstApiProviderResolver, GstApiProviderResolver>();
        services.AddTransient<IGstLookupService, GstLookupService>();
        services.AddTransient<IEInvoiceJsonGenerator, EInvoiceJsonGenerator>();
        services.AddSingleton<GovernmentGstProvider>(serviceProvider =>
        {
            var httpClient = httpClientFactory?.Invoke(serviceProvider) ?? CreateDefaultHttpClient(serviceProvider.GetRequiredService<GstApiProviderOptions>());
            return new GovernmentGstProvider(
                httpClient,
                serviceProvider.GetRequiredService<GstApiProviderOptions>(),
                serviceProvider.GetRequiredService<GstApiRetryOptions>());
        });
        services.AddSingleton<ThirdPartyGstApiProvider>(serviceProvider =>
        {
            var httpClient = httpClientFactory?.Invoke(serviceProvider) ?? CreateDefaultHttpClient(serviceProvider.GetRequiredService<GstApiProviderOptions>());
            return new ThirdPartyGstApiProvider(
                httpClient,
                serviceProvider.GetRequiredService<GstApiProviderOptions>(),
                serviceProvider.GetRequiredService<GstApiRetryOptions>());
        });
        services.AddSingleton<MockGstApiProvider>(serviceProvider =>
            new MockGstApiProvider(serviceProvider.GetRequiredService<IReadOnlyDictionary<string, GstTaxPayer>>()));
        services.AddSingleton<IGstApiProvider>(serviceProvider => serviceProvider.GetRequiredService<GovernmentGstProvider>());
        services.AddSingleton<IGstApiProvider>(serviceProvider => serviceProvider.GetRequiredService<ThirdPartyGstApiProvider>());
        services.AddSingleton<IGstApiProvider>(serviceProvider => serviceProvider.GetRequiredService<MockGstApiProvider>());

        return services;
    }

    /// <summary>
    /// Registers an additional GST API provider implementation.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGstApiProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IGstApiProvider
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IGstApiProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Creates a default HTTP client for GST API providers.
    /// </summary>
    /// <param name="options">The provider configuration.</param>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    private static HttpClient CreateDefaultHttpClient(GstApiProviderOptions options)
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds)
        };
    }
}
