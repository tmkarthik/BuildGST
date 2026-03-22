using System.Collections.Generic;

namespace BuildGST.Abstractions.Models;

/// <summary>
/// Represents a GST invoice used to generate GSTN e-invoice JSON.
/// </summary>
public sealed class GstInvoice
{
    /// <summary>
    /// Gets or sets the invoice version.
    /// </summary>
    public string Version { get; set; } = "1.1";

    /// <summary>
    /// Gets or sets invoice metadata.
    /// </summary>
    public GstInvoiceMetadata Metadata { get; set; } = new GstInvoiceMetadata();

    /// <summary>
    /// Gets or sets seller details.
    /// </summary>
    public GstInvoiceParty Seller { get; set; } = new GstInvoiceParty();

    /// <summary>
    /// Gets or sets buyer details.
    /// </summary>
    public GstInvoiceParty Buyer { get; set; } = new GstInvoiceParty();

    /// <summary>
    /// Gets or sets the invoice items.
    /// </summary>
    public IList<GstInvoiceItem> Items { get; set; } = new List<GstInvoiceItem>();

    /// <summary>
    /// Gets or sets invoice totals.
    /// </summary>
    public GstInvoiceTotals Totals { get; set; } = new GstInvoiceTotals();
}

/// <summary>
/// Represents invoice metadata.
/// </summary>
public sealed class GstInvoiceMetadata
{
    public string SupplyType { get; set; } = "B2B";

    public string DocumentType { get; set; } = "INV";

    public string InvoiceNumber { get; set; } = string.Empty;

    public string InvoiceDate { get; set; } = string.Empty;
}

/// <summary>
/// Represents a seller or buyer in the invoice.
/// </summary>
public sealed class GstInvoiceParty
{
    public string Gstin { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int PostalCode { get; set; }

    public string StateCode { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single invoice item.
/// </summary>
public sealed class GstInvoiceItem
{
    public string SerialNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HsnCode { get; set; } = string.Empty;

    public bool IsService { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TaxableAmount { get; set; }

    public GstInvoiceTax Tax { get; set; } = new GstInvoiceTax();

    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Represents tax details for an invoice item.
/// </summary>
public sealed class GstInvoiceTax
{
    public decimal GstRate { get; set; }

    public decimal CgstAmount { get; set; }

    public decimal SgstAmount { get; set; }

    public decimal IgstAmount { get; set; }
}

/// <summary>
/// Represents invoice totals.
/// </summary>
public sealed class GstInvoiceTotals
{
    public decimal AssessableValue { get; set; }

    public decimal CgstValue { get; set; }

    public decimal SgstValue { get; set; }

    public decimal IgstValue { get; set; }

    public decimal TotalInvoiceValue { get; set; }
}
