using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

/// <summary>
/// Represents a government-backed GST provider implementation.
/// </summary>
public sealed class GovernmentGstApiProvider : GstApiProviderBase
{
    private readonly GstApiProviderOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GovernmentGstApiProvider"/> class.
    /// </summary>
    public GovernmentGstApiProvider(HttpClient httpClient, GstApiProviderOptions options, GstApiRetryOptions? retryOptions = null)
        : base(httpClient, retryOptions)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override string Name => "government";

    /// <inheritdoc />
    protected override Task<HttpRequestMessage> CreateRequestAsync(string gstin, CancellationToken cancellationToken)
    {
        var baseUri = EnsureBaseUri();
        var path = _options.LookupPathTemplate.Replace("{gstin}", Uri.EscapeDataString(gstin));
        return Task.FromResult(new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path)));
    }

    /// <inheritdoc />
    protected override Task<GstTaxPayer> ParseTaxPayerAsync(string gstin, string payload, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GstTaxPayer
        {
            Gstin = gstin,
            LegalName = "Government Provider Taxpayer",
            RegistrationStatus = "Unknown",
            TradeName = payload
        });
    }

    private Uri EnsureBaseUri()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("Government provider base URL is not configured.");
        }

        var normalized = _options.BaseUrl.EndsWith("/", StringComparison.Ordinal) ? _options.BaseUrl : _options.BaseUrl + "/";
        return new Uri(normalized, UriKind.Absolute);
    }
}
