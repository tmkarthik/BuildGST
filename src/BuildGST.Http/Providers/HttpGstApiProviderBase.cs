using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

public abstract class HttpGstApiProviderBase : IGstApiProvider
{
    protected HttpGstApiProviderBase(HttpClient httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public abstract string Name { get; }

    protected HttpClient HttpClient { get; }

    public async Task<GstLookupResponse> LookupAsync(GstLookupRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        using (var httpRequest = await CreateRequestAsync(request, cancellationToken).ConfigureAwait(false))
        using (var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
        {
            var payload = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return await ParseResponseAsync(request, httpResponse, payload, cancellationToken).ConfigureAwait(false);
        }
    }

    protected abstract Task<HttpRequestMessage> CreateRequestAsync(GstLookupRequest request, CancellationToken cancellationToken);

    protected virtual Task<GstLookupResponse> ParseResponseAsync(
        GstLookupRequest request,
        HttpResponseMessage response,
        string payload,
        CancellationToken cancellationToken)
    {
        var result = new GstLookupResponse
        {
            Gstin = request.Gstin,
            ProviderName = Name,
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            RawPayload = payload,
            Message = response.ReasonPhrase
        };

        return Task.FromResult(result);
    }
}
