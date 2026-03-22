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

        var validationError = _gstinValidator.GetValidationError(request.Gstin);
        if (!string.IsNullOrEmpty(validationError))
        {
            throw new ArgumentException(validationError, nameof(request));
        }

        request.Gstin = Validation.GstinValidator.Normalize(request.Gstin);

        var provider = _providerResolver.Resolve();
        var taxPayer = await provider.GetTaxPayerAsync(request.Gstin, cancellationToken).ConfigureAwait(false);
        return new GstLookupResponse
        {
            Gstin = request.Gstin,
            ProviderName = provider.Name,
            IsSuccess = true,
            Taxpayer = taxPayer
        };
    }
}
