using System;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstLookupServiceTests
{
    [Fact]
    public async Task LookupAsync_ShouldNormalizeGstinAndUseResolvedProvider()
    {
        var provider = new FakeProvider();
        var options = new GstApiProviderOptions { DefaultProvider = provider.Name };
        var resolver = new GstApiProviderResolver(new[] { provider }, options);
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
        var options = new GstApiProviderOptions { DefaultProvider = provider.Name };
        var resolver = new GstApiProviderResolver(new[] { provider }, options);
        var service = new GstLookupService(new GstinValidator(), resolver);

        await Assert.ThrowsAsync<ArgumentException>(() => service.LookupAsync(new GstLookupRequest { Gstin = "INVALID" }));
        Assert.Null(provider.LastSeenGstin);
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
    }

    private sealed class FakeProvider : IGstApiProvider
    {
        public string Name => "fake";

        public string? LastSeenGstin { get; private set; }

        public Task<GstLookupResponse> LookupAsync(GstLookupRequest request, CancellationToken cancellationToken = default)
        {
            LastSeenGstin = request.Gstin;
            return Task.FromResult(new GstLookupResponse
            {
                IsSuccess = true,
                Gstin = request.Gstin,
                ProviderName = Name
            });
        }
    }
}
