using System;

namespace BuildGST.Abstractions.Exceptions;

/// <summary>
/// Represents a validation failure for a GSTIN.
/// </summary>
public sealed class InvalidGstinException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidGstinException"/> class.
    /// </summary>
    /// <param name="gstin">The invalid GSTIN.</param>
    /// <param name="message">The validation error.</param>
    public InvalidGstinException(string gstin, string message)
        : base(message)
    {
        Gstin = gstin ?? string.Empty;
    }

    /// <summary>
    /// Gets the invalid GSTIN value.
    /// </summary>
    public string Gstin { get; }
}
