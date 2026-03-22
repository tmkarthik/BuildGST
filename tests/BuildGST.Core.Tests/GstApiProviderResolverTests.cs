using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstApiProviderResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnGovernmentProvider_WhenConfigured()
    {
        var governmentProvider = new FakeProvider("government");
        var serviceProvider = new StubServiceProvider(new[] { governmentProvider, new FakeProvider("third-party") });
        var options = Options.Create(new GstApiProviderOptions
        {
            ProviderType = ProviderType.Government
        });
        var resolver = new GstApiProviderResolver(serviceProvider, options);

        var provider = resolver.Resolve();

        Assert.Same(governmentProvider, provider);
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenConfiguredProviderIsMissing()
    {
        var serviceProvider = new StubServiceProvider(new[] { new FakeProvider("government") });
        var options = Options.Create(new GstApiProviderOptions
        {
            ProviderType = (ProviderType)999
        });
        var resolver = new GstApiProviderResolver(serviceProvider, options);

        var exception = Assert.Throws<InvalidOperationException>(() => resolver.Resolve());

        Assert.Contains("No GST API provider is registered", exception.Message);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        private readonly IEnumerable<IGstApiProvider> _providers;

        public StubServiceProvider(IEnumerable<IGstApiProvider> providers)
        {
            _providers = providers;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEnumerable<IGstApiProvider>))
            {
                return _providers;
            }

            return null;
        }
    }

    private sealed class FakeProvider : IGstApiProvider
    {
        public FakeProvider(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken)
        {
            return Task.FromResult(new GstTaxPayer { Gstin = gstin });
        }
    }
}
