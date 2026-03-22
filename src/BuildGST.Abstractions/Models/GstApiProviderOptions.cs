namespace BuildGST.Abstractions.Models;

/// <summary>
/// Represents configuration for GST API providers and provider selection.
/// </summary>
public sealed class GstApiProviderOptions
{
    /// <summary>
    /// Gets or sets the active provider strategy.
    /// </summary>
    public ProviderType Provider { get; set; } = ProviderType.ThirdParty;

    /// <summary>
    /// Gets or sets the active provider strategy.
    /// </summary>
    public ProviderType ProviderType
    {
        get => Provider;
        set => Provider = value;
    }

    /// <summary>
    /// Gets or sets the provider base URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider lookup path template.
    /// </summary>
    public string LookupPathTemplate { get; set; } = "api/gst/{gstin}";

    /// <summary>
    /// Gets or sets the API key when required by the provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API key header name.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "x-api-key";

    /// <summary>
    /// Gets or sets the provider timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
