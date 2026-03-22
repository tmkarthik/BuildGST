namespace BuildGST.Abstractions.Models;

/// <summary>
/// Represents GST taxpayer information returned by a provider.
/// </summary>
public sealed class GstTaxPayer
{
    /// <summary>
    /// Gets or sets the GSTIN.
    /// </summary>
    public string Gstin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the legal name.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trade name.
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Gets or sets the registration status.
    /// </summary>
    public string? RegistrationStatus { get; set; }

    /// <summary>
    /// Gets or sets the taxpayer type.
    /// </summary>
    public string? TaxpayerType { get; set; }

    /// <summary>
    /// Gets or sets the postal address.
    /// </summary>
    public PostalAddress? Address { get; set; }
}
