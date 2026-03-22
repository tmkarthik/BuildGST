using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Exceptions;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Core.Services;

/// <summary>
/// Coordinates GST validation, provider resolution, and taxpayer lookup.
/// </summary>
public sealed class GstLookupService : IGstLookupService
{
    private readonly IGstinValidator _gstinValidator;
    private readonly IGstApiProviderResolver _providerResolver;
    private readonly IEnumerable<IGstLookupLogger> _loggers;

    /// <summary>
    /// Initializes a new instance of the <see cref="GstLookupService"/> class.
    /// </summary>
    /// <param name="gstinValidator">The GST validator.</param>
    /// <param name="providerResolver">The provider resolver.</param>
    /// <param name="loggers">Optional logging extension points.</param>
    public GstLookupService(
        IGstinValidator gstinValidator,
        IGstApiProviderResolver providerResolver,
        IEnumerable<IGstLookupLogger>? loggers = null)
    {
        _gstinValidator = gstinValidator ?? throw new ArgumentNullException(nameof(gstinValidator));
        _providerResolver = providerResolver ?? throw new ArgumentNullException(nameof(providerResolver));
        _loggers = loggers ?? Array.Empty<IGstLookupLogger>();
    }

    /// <inheritdoc />
    public async Task<GstTaxPayer> LookupAsync(string gstin, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationError = _gstinValidator.GetValidationError(gstin);
        if (!string.IsNullOrEmpty(validationError))
        {
            NotifyValidationFailed(gstin ?? string.Empty, validationError);
            throw new InvalidGstinException(gstin ?? string.Empty, validationError);
        }

        var normalizedGstin = Validation.GstinValidator.Normalize(gstin);
        NotifyLookupStarted(normalizedGstin);

        try
        {
            var provider = _providerResolver.Resolve();
            var taxPayer = await provider.GetTaxPayerAsync(normalizedGstin, cancellationToken).ConfigureAwait(false);
            NotifyLookupSucceeded(normalizedGstin, provider.Name);
            return taxPayer;
        }
        catch (Exception exception) when (exception is GstApiException || exception is InvalidOperationException)
        {
            NotifyLookupFailed(normalizedGstin, exception);
            throw new GstLookupFailedException(normalizedGstin, "GST lookup failed due to provider or API error.", exception);
        }
    }

    private void NotifyLookupStarted(string gstin)
    {
        foreach (var logger in _loggers)
        {
            logger.LookupStarted(gstin);
        }
    }

    private void NotifyValidationFailed(string gstin, string error)
    {
        foreach (var logger in _loggers)
        {
            logger.ValidationFailed(gstin, error);
        }
    }

    private void NotifyLookupSucceeded(string gstin, string providerName)
    {
        foreach (var logger in _loggers)
        {
            logger.LookupSucceeded(gstin, providerName);
        }
    }

    private void NotifyLookupFailed(string gstin, Exception exception)
    {
        foreach (var logger in _loggers)
        {
            logger.LookupFailed(gstin, exception);
        }
    }
}
