using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Validation;

namespace BuildGST.Core.Services;

/// <summary>
/// Generates GSTN-compatible e-invoice JSON payloads.
/// </summary>
public sealed class EInvoiceJsonGenerator : IEInvoiceJsonGenerator
{
    private const decimal Tolerance = 0.01m;
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly IGstinValidator _gstinValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EInvoiceJsonGenerator"/> class.
    /// </summary>
    /// <param name="gstinValidator">The GSTIN validator used for seller and buyer validation.</param>
    public EInvoiceJsonGenerator(IGstinValidator gstinValidator)
    {
        _gstinValidator = gstinValidator ?? throw new ArgumentNullException(nameof(gstinValidator));
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(GstInvoice invoice, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (invoice == null)
        {
            throw new ArgumentNullException(nameof(invoice));
        }

        ValidateInvoice(invoice);

        var payload = CreatePayload(invoice);
        using (var stream = new MemoryStream())
        {
            await JsonSerializer.SerializeAsync(stream, payload, SerializerOptions, token).ConfigureAwait(false);
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }

    private void ValidateInvoice(GstInvoice invoice)
    {
        if (invoice.Items == null || invoice.Items.Count == 0)
        {
            throw new ArgumentException("Invoice must contain at least one item.", nameof(invoice));
        }

        if (string.IsNullOrWhiteSpace(invoice.Metadata.InvoiceNumber))
        {
            throw new ArgumentException("Invoice number is required.", nameof(invoice));
        }

        ValidateParty(invoice.Seller, "Seller", invoice);
        ValidateParty(invoice.Buyer, "Buyer", invoice);
        ValidateTaxCalculations(invoice);
        ValidateTotals(invoice);
    }

    private void ValidateParty(GstInvoiceParty party, string partyName, GstInvoice invoice)
    {
        if (party == null)
        {
            throw new ArgumentException($"{partyName} details are required.", nameof(invoice));
        }

        var error = _gstinValidator.GetValidationError(party.Gstin);
        if (!string.IsNullOrEmpty(error))
        {
            throw new ArgumentException($"{partyName} GSTIN is invalid. {error}", nameof(invoice));
        }
    }

    private void ValidateTaxCalculations(GstInvoice invoice)
    {
        foreach (var item in invoice.Items)
        {
            var expectedTaxable = Round(item.Quantity * item.UnitPrice);
            if (!AreEqual(item.TaxableAmount, expectedTaxable))
            {
                throw new InvalidOperationException($"Taxable amount mismatch for item '{item.SerialNumber}'.");
            }

            var expectedTax = Round(item.TaxableAmount * item.Tax.GstRate / 100m);
            var actualTax = Round(item.Tax.CgstAmount + item.Tax.SgstAmount + item.Tax.IgstAmount);
            if (!AreEqual(expectedTax, actualTax))
            {
                throw new InvalidOperationException($"Tax calculation mismatch for item '{item.SerialNumber}'.");
            }

            var expectedItemTotal = Round(item.TaxableAmount + actualTax);
            if (!AreEqual(item.TotalAmount, expectedItemTotal))
            {
                throw new InvalidOperationException($"Total amount mismatch for item '{item.SerialNumber}'.");
            }
        }
    }

    private void ValidateTotals(GstInvoice invoice)
    {
        var assessableValue = Round(invoice.Items.Sum(item => item.TaxableAmount));
        var cgstValue = Round(invoice.Items.Sum(item => item.Tax.CgstAmount));
        var sgstValue = Round(invoice.Items.Sum(item => item.Tax.SgstAmount));
        var igstValue = Round(invoice.Items.Sum(item => item.Tax.IgstAmount));
        var totalInvoiceValue = Round(invoice.Items.Sum(item => item.TotalAmount));

        if (!AreEqual(invoice.Totals.AssessableValue, assessableValue)
            || !AreEqual(invoice.Totals.CgstValue, cgstValue)
            || !AreEqual(invoice.Totals.SgstValue, sgstValue)
            || !AreEqual(invoice.Totals.IgstValue, igstValue)
            || !AreEqual(invoice.Totals.TotalInvoiceValue, totalInvoiceValue))
        {
            throw new InvalidOperationException("Invoice totals validation failed.");
        }
    }

    private static GstnInvoicePayload CreatePayload(GstInvoice invoice)
    {
        return new GstnInvoicePayload
        {
            Version = invoice.Version,
            InvoiceMetadata = new GstnInvoiceMetadataPayload
            {
                SupplyType = invoice.Metadata.SupplyType,
                DocumentType = invoice.Metadata.DocumentType,
                InvoiceNumber = invoice.Metadata.InvoiceNumber,
                InvoiceDate = invoice.Metadata.InvoiceDate
            },
            Seller = MapParty(invoice.Seller),
            Buyer = MapParty(invoice.Buyer),
            ItemList = invoice.Items.Select(MapItem).ToArray(),
            Totals = new GstnInvoiceTotalsPayload
            {
                AssessableValue = invoice.Totals.AssessableValue,
                CgstValue = invoice.Totals.CgstValue,
                SgstValue = invoice.Totals.SgstValue,
                IgstValue = invoice.Totals.IgstValue,
                TotalInvoiceValue = invoice.Totals.TotalInvoiceValue
            }
        };
    }

    private static GstnPartyPayload MapParty(GstInvoiceParty party)
    {
        return new GstnPartyPayload
        {
            Gstin = GstinValidator.Normalize(party.Gstin),
            LegalName = party.LegalName,
            TradeName = party.TradeName,
            AddressLine1 = party.AddressLine1,
            Location = party.Location,
            PostalCode = party.PostalCode,
            StateCode = party.StateCode
        };
    }

    private static GstnItemPayload MapItem(GstInvoiceItem item)
    {
        return new GstnItemPayload
        {
            SerialNumber = item.SerialNumber,
            Description = item.Description,
            HsnCode = item.HsnCode,
            IsService = item.IsService ? "Y" : "N",
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TaxableAmount = item.TaxableAmount,
            TotalAmount = item.TotalAmount,
            TaxDetails = new GstnTaxPayload
            {
                GstRate = item.Tax.GstRate,
                CgstAmount = item.Tax.CgstAmount,
                SgstAmount = item.Tax.SgstAmount,
                IgstAmount = item.Tax.IgstAmount
            }
        };
    }

    private static bool AreEqual(decimal left, decimal right)
    {
        return Math.Abs(left - right) <= Tolerance;
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private sealed class GstnInvoicePayload
    {
        public string Version { get; set; } = string.Empty;

        public GstnInvoiceMetadataPayload InvoiceMetadata { get; set; } = new GstnInvoiceMetadataPayload();

        public GstnPartyPayload Seller { get; set; } = new GstnPartyPayload();

        public GstnPartyPayload Buyer { get; set; } = new GstnPartyPayload();

        public GstnItemPayload[] ItemList { get; set; } = Array.Empty<GstnItemPayload>();

        public GstnInvoiceTotalsPayload Totals { get; set; } = new GstnInvoiceTotalsPayload();
    }

    private sealed class GstnInvoiceMetadataPayload
    {
        public string SupplyType { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public string InvoiceDate { get; set; } = string.Empty;
    }

    private sealed class GstnPartyPayload
    {
        public string Gstin { get; set; } = string.Empty;

        public string LegalName { get; set; } = string.Empty;

        public string? TradeName { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int PostalCode { get; set; }

        public string StateCode { get; set; } = string.Empty;
    }

    private sealed class GstnItemPayload
    {
        public string SerialNumber { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string HsnCode { get; set; } = string.Empty;

        public string IsService { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxableAmount { get; set; }

        public GstnTaxPayload TaxDetails { get; set; } = new GstnTaxPayload();

        public decimal TotalAmount { get; set; }
    }

    private sealed class GstnTaxPayload
    {
        public decimal GstRate { get; set; }

        public decimal CgstAmount { get; set; }

        public decimal SgstAmount { get; set; }

        public decimal IgstAmount { get; set; }
    }

    private sealed class GstnInvoiceTotalsPayload
    {
        public decimal AssessableValue { get; set; }

        public decimal CgstValue { get; set; }

        public decimal SgstValue { get; set; }

        public decimal IgstValue { get; set; }

        public decimal TotalInvoiceValue { get; set; }
    }
}
