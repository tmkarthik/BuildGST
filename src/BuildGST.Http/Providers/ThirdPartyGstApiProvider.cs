using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

/// <summary>
/// Represents a third-party GST provider implementation.
/// </summary>
public sealed class ThirdPartyGstApiProvider : GstApiProviderBase
{
    private readonly GstApiProviderOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyGstApiProvider"/> class.
    /// </summary>
    public ThirdPartyGstApiProvider(HttpClient httpClient, GstApiProviderOptions options, GstApiRetryOptions? retryOptions = null)
        : base(httpClient, retryOptions)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override string Name => "third-party";

    /// <inheritdoc />
    protected override Task<HttpRequestMessage> CreateRequestAsync(string gstin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("Third-party provider base URL is not configured.");
        }

        var normalized = _options.BaseUrl.EndsWith("/", StringComparison.Ordinal) ? _options.BaseUrl : _options.BaseUrl + "/";
        var uri = new Uri(new Uri(normalized, UriKind.Absolute), _options.LookupPathTemplate.Replace("{gstin}", Uri.EscapeDataString(gstin)));
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKey);
        }

        return Task.FromResult(request);
    }

    /// <inheritdoc />
    protected override Task<GstTaxPayer> ParseTaxPayerAsync(string gstin, string payload, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GstTaxPayer
        {
            Gstin = gstin,
            LegalName = "Third Party Provider Taxpayer",
            RegistrationStatus = "Unknown",
            TradeName = payload
        });
    }
}
