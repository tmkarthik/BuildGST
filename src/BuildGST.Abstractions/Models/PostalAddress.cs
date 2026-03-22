namespace BuildGST.Abstractions.Models;

public sealed class PostalAddress
{
    public string? BuildingNumber { get; set; }

    public string? Street { get; set; }

    public string? Location { get; set; }

    public string City { get; set; } = string.Empty;

    public string StateCode { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
}
