namespace BuildGST.Abstractions.Models;

public sealed class TaxpayerInfo
{
    public string Gstin { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? RegistrationStatus { get; set; }

    public string? TaxpayerType { get; set; }

    public PostalAddress? Address { get; set; }
}
