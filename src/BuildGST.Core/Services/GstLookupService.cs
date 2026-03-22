using System;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Core.Services;

public sealed class GstLookupService : IGstLookupService
{
    private readonly IGstinValidator _gstinValidator;
    private readonly IGstApiProviderResolver _providerResolver;

    public GstLookupService(IGstinValidator gstinValidator, IGstApiProviderResolver providerResolver)
    {
        _gstinValidator = gstinValidator ?? throw new ArgumentNullException(nameof(gstinValidator));
        _providerResolver = providerResolver ?? throw new ArgumentNullException(nameof(providerResolver));
    }

    public async Task<GstLookupResponse> LookupAsync(GstLookupRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var validationResult = _gstinValidator.Validate(request.Gstin);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(validationResult.ErrorMessage, nameof(request));
        }

        request.Gstin = validationResult.NormalizedGstin;

        var provider = _providerResolver.Resolve(request.ProviderName);
        var response = await provider.LookupAsync(request, cancellationToken).ConfigureAwait(false);
        response.Gstin = request.Gstin;
        response.ProviderName = provider.Name;

        return response;
    }
}
