using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BuildGST.Abstractions.Models;

[DataContract]
public sealed class EInvoiceDocument
{
    [DataMember(Name = "Version", EmitDefaultValue = false)]
    public string Version { get; set; } = "1.1";

    [DataMember(Name = "TranDtls", EmitDefaultValue = false)]
    public TransactionDetails TransactionDetails { get; set; } = new TransactionDetails();

    [DataMember(Name = "DocDtls", EmitDefaultValue = false)]
    public DocumentDetails DocumentDetails { get; set; } = new DocumentDetails();

    [DataMember(Name = "SellerDtls", EmitDefaultValue = false)]
    public PartyDetails SellerDetails { get; set; } = new PartyDetails();

    [DataMember(Name = "BuyerDtls", EmitDefaultValue = false)]
    public PartyDetails BuyerDetails { get; set; } = new PartyDetails();

    [DataMember(Name = "ValDtls", EmitDefaultValue = false)]
    public InvoiceValueDetails ValueDetails { get; set; } = new InvoiceValueDetails();

    [DataMember(Name = "ItemList", EmitDefaultValue = false)]
    public IList<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}

[DataContract]
public sealed class TransactionDetails
{
    [DataMember(Name = "TaxSch", EmitDefaultValue = false)]
    public string TaxScheme { get; set; } = "GST";

    [DataMember(Name = "SupTyp", EmitDefaultValue = false)]
    public string SupplyType { get; set; } = "B2B";
}

[DataContract]
public sealed class DocumentDetails
{
    [DataMember(Name = "Typ", EmitDefaultValue = false)]
    public string DocumentType { get; set; } = "INV";

    [DataMember(Name = "No", EmitDefaultValue = false)]
    public string Number { get; set; } = string.Empty;

    [DataMember(Name = "Dt", EmitDefaultValue = false)]
    public string Date { get; set; } = string.Empty;
}

[DataContract]
public sealed class PartyDetails
{
    [DataMember(Name = "Gstin", EmitDefaultValue = false)]
    public string Gstin { get; set; } = string.Empty;

    [DataMember(Name = "LglNm", EmitDefaultValue = false)]
    public string LegalName { get; set; } = string.Empty;

    [DataMember(Name = "TrdNm", EmitDefaultValue = false)]
    public string? TradeName { get; set; }

    [DataMember(Name = "Addr1", EmitDefaultValue = false)]
    public string AddressLine1 { get; set; } = string.Empty;

    [DataMember(Name = "Loc", EmitDefaultValue = false)]
    public string Location { get; set; } = string.Empty;

    [DataMember(Name = "Pin", EmitDefaultValue = false)]
    public int PostalCode { get; set; }

    [DataMember(Name = "Stcd", EmitDefaultValue = false)]
    public string StateCode { get; set; } = string.Empty;
}

[DataContract]
public sealed class InvoiceValueDetails
{
    [DataMember(Name = "AssVal", EmitDefaultValue = false)]
    public decimal AssessableValue { get; set; }

    [DataMember(Name = "CgstVal", EmitDefaultValue = false)]
    public decimal CgstValue { get; set; }

    [DataMember(Name = "SgstVal", EmitDefaultValue = false)]
    public decimal SgstValue { get; set; }

    [DataMember(Name = "IgstVal", EmitDefaultValue = false)]
    public decimal IgstValue { get; set; }

    [DataMember(Name = "TotInvVal", EmitDefaultValue = false)]
    public decimal TotalInvoiceValue { get; set; }
}

[DataContract]
public sealed class InvoiceItem
{
    [DataMember(Name = "SlNo", EmitDefaultValue = false)]
    public string SerialNumber { get; set; } = string.Empty;

    [DataMember(Name = "PrdDesc", EmitDefaultValue = false)]
    public string Description { get; set; } = string.Empty;

    [DataMember(Name = "IsServc", EmitDefaultValue = false)]
    public string IsService { get; set; } = "N";

    [DataMember(Name = "HsnCd", EmitDefaultValue = false)]
    public string HsnCode { get; set; } = string.Empty;

    [DataMember(Name = "Qty", EmitDefaultValue = false)]
    public decimal Quantity { get; set; }

    [DataMember(Name = "UnitPrice", EmitDefaultValue = false)]
    public decimal UnitPrice { get; set; }

    [DataMember(Name = "TotAmt", EmitDefaultValue = false)]
    public decimal TotalAmount { get; set; }

    [DataMember(Name = "GstRt", EmitDefaultValue = false)]
    public decimal GstRate { get; set; }

    [DataMember(Name = "AssAmt", EmitDefaultValue = false)]
    public decimal AssessableAmount { get; set; }
}
