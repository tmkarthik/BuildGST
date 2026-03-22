using System.Text.Json.Serialization;

namespace BuildGST.Http.Dtos;

/// <summary>
/// Represents the taxpayer payload returned by a government GST lookup API.
/// </summary>
public sealed class GstLookupApiResponse
{
    /// <summary>
    /// Gets or sets the GSTIN.
    /// </summary>
    [JsonPropertyName("gstin")]
    public string? Gstin { get; set; }

    /// <summary>
    /// Gets or sets the legal name.
    /// </summary>
    [JsonPropertyName("legalName")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Gets or sets the trade name.
    /// </summary>
    [JsonPropertyName("tradeName")]
    public string? TradeName { get; set; }

    /// <summary>
    /// Gets or sets the registration status.
    /// </summary>
    [JsonPropertyName("registrationStatus")]
    public string? RegistrationStatus { get; set; }

    /// <summary>
    /// Gets or sets the taxpayer type.
    /// </summary>
    [JsonPropertyName("taxpayerType")]
    public string? TaxpayerType { get; set; }

    /// <summary>
    /// Gets or sets the taxpayer address.
    /// </summary>
    [JsonPropertyName("address")]
    public GstAddressDto? Address { get; set; }
}
