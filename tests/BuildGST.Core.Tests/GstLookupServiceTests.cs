using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using Microsoft.Extensions.Options;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstLookupServiceTests
{
    [Fact]
    public async Task LookupAsync_ShouldNormalizeGstinAndUseResolvedProvider()
    {
        var provider = new FakeProvider();
        var options = Options.Create(new GstApiProviderOptions { ProviderType = ProviderType.Government });
        var resolver = new GstApiProviderResolver(new StubServiceProvider(new[] { provider }), options);
        var service = new GstLookupService(new GstinValidator(), resolver);
        var gstin = BuildValidGstin("27ABCDE1234F1Z");

        var response = await service.LookupAsync(new GstLookupRequest { Gstin = gstin.ToLowerInvariant() });

        Assert.True(response.IsSuccess);
        Assert.Equal(gstin, response.Gstin);
        Assert.Equal(provider.Name, response.ProviderName);
        Assert.Equal(gstin, provider.LastSeenGstin);
    }

    [Fact]
    public async Task LookupAsync_ShouldRejectInvalidGstinBeforeCallingProvider()
    {
        var provider = new FakeProvider();
        var options = Options.Create(new GstApiProviderOptions { ProviderType = ProviderType.Government });
        var resolver = new GstApiProviderResolver(new StubServiceProvider(new[] { provider }), options);
        var service = new GstLookupService(new GstinValidator(), resolver);

        await Assert.ThrowsAsync<ArgumentException>(() => service.LookupAsync(new GstLookupRequest { Gstin = "INVALID" }));
        Assert.Null(provider.LastSeenGstin);
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
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
        public string Name => "government";

        public string? LastSeenGstin { get; private set; }

        public Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken)
        {
            LastSeenGstin = gstin;
            return Task.FromResult(new GstTaxPayer
            {
                Gstin = gstin,
                LegalName = "Fake Taxpayer"
            });
        }
    }
}
