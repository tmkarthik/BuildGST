using System;

namespace BuildGST.Abstractions.Exceptions;

/// <summary>
/// Represents a provider error while communicating with a GST API.
/// </summary>
public class GstApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GstApiException"/> class.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code when available.</param>
    /// <param name="isTransient">Whether the error is considered transient.</param>
    /// <param name="innerException">The inner exception.</param>
    public GstApiException(string providerName, string message, int? statusCode = null, bool isTransient = false, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderName = providerName;
        StatusCode = statusCode;
        IsTransient = isTransient;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// Gets the HTTP status code when available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the error is transient.
    /// </summary>
    public bool IsTransient { get; }
}
