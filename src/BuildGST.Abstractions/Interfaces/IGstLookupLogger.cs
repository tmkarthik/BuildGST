using System;

namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Provides logging extension points for GST lookup operations.
/// </summary>
public interface IGstLookupLogger
{
    /// <summary>
    /// Called when a GST lookup begins.
    /// </summary>
    /// <param name="gstin">The normalized GSTIN.</param>
    void LookupStarted(string gstin);

    /// <summary>
    /// Called when GST validation fails.
    /// </summary>
    /// <param name="gstin">The provided GSTIN.</param>
    /// <param name="error">The validation error.</param>
    void ValidationFailed(string gstin, string error);

    /// <summary>
    /// Called when a GST lookup succeeds.
    /// </summary>
    /// <param name="gstin">The normalized GSTIN.</param>
    /// <param name="providerName">The provider that handled the request.</param>
    void LookupSucceeded(string gstin, string providerName);

    /// <summary>
    /// Called when a GST lookup fails.
    /// </summary>
    /// <param name="gstin">The normalized GSTIN when available.</param>
    /// <param name="exception">The failure exception.</param>
    void LookupFailed(string gstin, Exception exception);
}
