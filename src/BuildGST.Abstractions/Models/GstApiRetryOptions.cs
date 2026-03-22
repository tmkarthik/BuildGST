namespace BuildGST.Abstractions.Models;

/// <summary>
/// Represents retry configuration for GST API providers.
/// </summary>
public sealed class GstApiRetryOptions
{
    /// <summary>
    /// Gets or sets the maximum retry count for transient failures.
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>
    /// Gets or sets the base retry delay in milliseconds.
    /// </summary>
    public int DelayMilliseconds { get; set; } = 250;
}
