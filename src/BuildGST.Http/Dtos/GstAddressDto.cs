using System.Text.Json.Serialization;

namespace BuildGST.Http.Dtos;

/// <summary>
/// Represents the address payload returned by a GST lookup API.
/// </summary>
public sealed class GstAddressDto
{
    /// <summary>
    /// Gets or sets the building number or door number.
    /// </summary>
    [JsonPropertyName("buildingNumber")]
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Gets or sets the street or road.
    /// </summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the locality.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the state code.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public string? StateCode { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
}
