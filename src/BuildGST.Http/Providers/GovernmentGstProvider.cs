using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using BuildGST.Abstractions.Exceptions;
using BuildGST.Abstractions.Models;
using BuildGST.Http.Dtos;

namespace BuildGST.Http.Providers;

/// <summary>
/// Government GST provider implementation that retrieves taxpayer details over HTTP.
/// </summary>
public sealed class GovernmentGstProvider : GstApiProviderBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GstApiProviderOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GovernmentGstProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the provider.</param>
    /// <param name="options">The provider configuration.</param>
    /// <param name="retryOptions">The retry configuration.</param>
    public GovernmentGstProvider(HttpClient httpClient, GstApiProviderOptions options, GstApiRetryOptions? retryOptions = null)
        : base(httpClient, retryOptions)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (_options.TimeoutSeconds > 0)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }
    }

    /// <inheritdoc />
    public override string Name => "government";

    /// <inheritdoc />
    protected override Task<HttpRequestMessage> CreateRequestAsync(string gstin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("Government provider base URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.LookupPathTemplate))
        {
            throw new InvalidOperationException("Government provider lookup path template is not configured.");
        }

        var baseUri = BuildBaseUri(_options.BaseUrl);
        var relativePath = _options.LookupPathTemplate.Replace("{gstin}", Uri.EscapeDataString(gstin));
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, relativePath));

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKey);
        }

        return Task.FromResult(request);
    }

    /// <inheritdoc />
    protected override async Task<GstTaxPayer> ParseTaxPayerAsync(string gstin, string payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new GstApiException(Name, "Government provider returned an empty response.");
        }

        using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload)))
        {
            var apiResponse = await JsonSerializer.DeserializeAsync<GstLookupApiResponse>(memoryStream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            if (apiResponse == null)
            {
                throw new GstApiException(Name, "Government provider response could not be deserialized.");
            }

            return MapToTaxPayer(gstin, apiResponse);
        }
    }

    private static GstTaxPayer MapToTaxPayer(string gstin, GstLookupApiResponse response)
    {
        var resolvedGstin = string.IsNullOrWhiteSpace(response.Gstin) ? gstin : response.Gstin!;

        return new GstTaxPayer
        {
            Gstin = resolvedGstin,
            LegalName = response.LegalName ?? string.Empty,
            TradeName = response.TradeName,
            RegistrationStatus = response.RegistrationStatus,
            TaxpayerType = response.TaxpayerType,
            Address = response.Address == null
                ? null
                : new PostalAddress
                {
                    BuildingNumber = response.Address.BuildingNumber,
                    Street = response.Address.Street,
                    Location = response.Address.Location,
                    City = response.Address.City ?? string.Empty,
                    StateCode = response.Address.StateCode ?? string.Empty,
                    PostalCode = response.Address.PostalCode ?? string.Empty
                }
        };
    }

    private static Uri BuildBaseUri(string baseUrl)
    {
        var normalized = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/";
        return new Uri(normalized, UriKind.Absolute);
    }
}
