using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Exceptions;
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
    public async Task LookupAsync_ShouldReturnTaxPayer_ForValidGstin()
    {
        var provider = new FakeProvider();
        var options = Options.Create(new GstApiProviderOptions { ProviderType = ProviderType.Government });
        var resolver = new GstApiProviderResolver(new StubServiceProvider(new[] { provider }), options);
        var logger = new FakeLookupLogger();
        var service = new GstLookupService(new GstinValidator(), resolver, new[] { logger });
        var gstin = BuildValidGstin("27ABCDE1234F1Z");

        var taxPayer = await service.LookupAsync(gstin.ToLowerInvariant());

        Assert.Equal(gstin, taxPayer.Gstin);
        Assert.Equal("Fake Taxpayer", taxPayer.LegalName);
        Assert.Equal(gstin, provider.LastSeenGstin);
        Assert.Equal(gstin, logger.StartedGstin);
        Assert.Equal(provider.Name, logger.SuccessProviderName);
    }

    [Fact]
    public async Task LookupAsync_ShouldRejectInvalidGstin()
    {
        var provider = new FakeProvider();
        var options = Options.Create(new GstApiProviderOptions { ProviderType = ProviderType.Government });
        var resolver = new GstApiProviderResolver(new StubServiceProvider(new[] { provider }), options);
        var logger = new FakeLookupLogger();
        var service = new GstLookupService(new GstinValidator(), resolver, new[] { logger });

        var exception = await Assert.ThrowsAsync<InvalidGstinException>(() => service.LookupAsync("INVALID"));

        Assert.Equal("GSTIN must contain exactly 15 characters.", exception.Message);
        Assert.Null(provider.LastSeenGstin);
        Assert.Equal("INVALID", logger.ValidationFailedGstin);
    }

    [Fact]
    public async Task LookupAsync_ShouldWrapProviderFailure()
    {
        var provider = new ThrowingProvider();
        var options = Options.Create(new GstApiProviderOptions { ProviderType = ProviderType.Government });
        var resolver = new GstApiProviderResolver(new StubServiceProvider(new[] { provider }), options);
        var logger = new FakeLookupLogger();
        var service = new GstLookupService(new GstinValidator(), resolver, new[] { logger });
        var gstin = BuildValidGstin("27ABCDE1234F1Z");

        var exception = await Assert.ThrowsAsync<GstLookupFailedException>(() => service.LookupAsync(gstin));

        Assert.Equal(gstin, exception.Gstin);
        Assert.NotNull(exception.InnerException);
        Assert.Equal(gstin, logger.FailedGstin);
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

    private sealed class ThrowingProvider : IGstApiProvider
    {
        public string Name => "government";

        public Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken)
        {
            throw new GstApiException(Name, "Provider failure.", 500, true);
        }
    }

    private sealed class FakeLookupLogger : IGstLookupLogger
    {
        public string? StartedGstin { get; private set; }

        public string? ValidationFailedGstin { get; private set; }

        public string? SuccessProviderName { get; private set; }

        public string? FailedGstin { get; private set; }

        public void LookupStarted(string gstin)
        {
            StartedGstin = gstin;
        }

        public void ValidationFailed(string gstin, string error)
        {
            ValidationFailedGstin = gstin;
        }

        public void LookupSucceeded(string gstin, string providerName)
        {
            SuccessProviderName = providerName;
        }

        public void LookupFailed(string gstin, Exception exception)
        {
            FailedGstin = gstin;
        }
    }
}
