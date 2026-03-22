using System;
using System.Net.Http;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using BuildGST.Http.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildGST.DependencyInjection;

public static class BuildGstServiceCollectionExtensions
{
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

        services.AddSingleton(options);
        services.AddSingleton<IOptions<GstApiProviderOptions>>(Options.Create(options));
        services.AddSingleton(retryOptions);
        services.AddSingleton<IGstinValidator, GstinValidator>();
        services.AddSingleton<IGstApiProviderResolver, GstApiProviderResolver>();
        services.AddTransient<IGstLookupService, GstLookupService>();
        services.AddTransient<IEInvoiceJsonGenerator, EInvoiceJsonGenerator>();
        services.AddSingleton<ThirdPartyGstApiProvider>(serviceProvider =>
        {
            var httpClient = httpClientFactory?.Invoke(serviceProvider) ?? CreateDefaultHttpClient(serviceProvider.GetRequiredService<GstApiProviderOptions>());
            return new ThirdPartyGstApiProvider(
                httpClient,
                serviceProvider.GetRequiredService<GstApiProviderOptions>(),
                serviceProvider.GetRequiredService<GstApiRetryOptions>());
        });
        services.AddSingleton<IGstApiProvider>(serviceProvider => serviceProvider.GetRequiredService<ThirdPartyGstApiProvider>());

        return services;
    }

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

    private static HttpClient CreateDefaultHttpClient(GstApiProviderOptions options)
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds)
        };
    }
}
