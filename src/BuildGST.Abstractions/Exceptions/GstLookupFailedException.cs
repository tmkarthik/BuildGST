using System;

namespace BuildGST.Abstractions.Exceptions;

/// <summary>
/// Represents a GST lookup failure caused by an API/provider error.
/// </summary>
public sealed class GstLookupFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GstLookupFailedException"/> class.
    /// </summary>
    /// <param name="gstin">The GSTIN being looked up.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="innerException">The underlying provider exception.</param>
    public GstLookupFailedException(string gstin, string message, Exception innerException)
        : base(message, innerException)
    {
        Gstin = gstin ?? string.Empty;
    }

    /// <summary>
    /// Gets the GSTIN associated with the lookup failure.
    /// </summary>
    public string Gstin { get; }
}
