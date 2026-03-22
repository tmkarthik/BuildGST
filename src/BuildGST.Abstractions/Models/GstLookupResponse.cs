using System.Collections.Generic;

namespace BuildGST.Abstractions.Models;

public sealed class GstLookupResponse
{
    public bool IsSuccess { get; set; }

    public string Gstin { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public string? Message { get; set; }

    public string? RawPayload { get; set; }

    public GstTaxPayer? Taxpayer { get; set; }

    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
