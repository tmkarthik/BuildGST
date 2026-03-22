namespace BuildGST.Abstractions.Models;

public sealed class GstApiProviderOptions
{
    public string DefaultProvider { get; set; } = "configurable-http";

    public string BaseUrl { get; set; } = string.Empty;

    public string LookupPathTemplate { get; set; } = "api/gst/{gstin}";

    public string? ApiKey { get; set; }

    public string ApiKeyHeaderName { get; set; } = "x-api-key";

    public int TimeoutSeconds { get; set; } = 30;
}
