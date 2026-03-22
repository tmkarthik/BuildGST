using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

public sealed class ConfigurableHttpGstApiProvider : HttpGstApiProviderBase
{
    private readonly GstApiProviderOptions _options;

    public ConfigurableHttpGstApiProvider(HttpClient httpClient, GstApiProviderOptions options)
        : base(httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public override string Name => "configurable-http";

    protected override Task<HttpRequestMessage> CreateRequestAsync(GstLookupRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("GST API base URL is not configured.");
        }

        var baseUri = _options.BaseUrl.EndsWith("/", StringComparison.Ordinal) ? _options.BaseUrl : _options.BaseUrl + "/";
        var relativePath = _options.LookupPathTemplate.Replace("{gstin}", Uri.EscapeDataString(request.Gstin));
        var requestUri = new Uri(new Uri(baseUri, UriKind.Absolute), relativePath);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            httpRequest.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            httpRequest.Headers.TryAddWithoutValidation("X-Correlation-Id", request.CorrelationId);
        }

        return Task.FromResult(httpRequest);
    }
}
