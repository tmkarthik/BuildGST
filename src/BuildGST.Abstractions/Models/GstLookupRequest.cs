namespace BuildGST.Abstractions.Models;

public sealed class GstLookupRequest
{
    public string Gstin { get; set; } = string.Empty;

    public string? ProviderName { get; set; }

    public string? CorrelationId { get; set; }
}
