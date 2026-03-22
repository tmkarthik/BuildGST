using System;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class EInvoiceJsonGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ShouldProduceValidInvoiceJson()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();

        var json = await generator.GenerateAsync(invoice, CancellationToken.None);

        Assert.Contains("\"seller\"", json);
        Assert.Contains("\"buyer\"", json);
        Assert.Contains("\"itemList\"", json);
        Assert.Contains("\"invoiceMetadata\"", json);
        Assert.Contains("\"totals\"", json);
        Assert.Contains("\"taxDetails\"", json);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_ForNullInvoice()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());

        await Assert.ThrowsAsync<ArgumentNullException>(() => generator.GenerateAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_ForEmptyItems()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();
        invoice.Items.Clear();

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateAsync(invoice, CancellationToken.None));

        Assert.Equal("Invoice must contain at least one item. (Parameter 'invoice')", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_WhenTaxCalculationIsInvalid()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();
        invoice.Items[0].Tax.CgstAmount = 10m;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateAsync(invoice, CancellationToken.None));

        Assert.Equal("Tax calculation mismatch for item '1'.", exception.Message);
    }

    private static GstInvoice CreateValidInvoice()
    {
        return new GstInvoice
        {
            Metadata = new GstInvoiceMetadata
            {
                InvoiceNumber = "INV-1001",
                InvoiceDate = "22/03/2026",
                DocumentType = "INV",
                SupplyType = "B2B"
            },
            Seller = new GstInvoiceParty
            {
                Gstin = BuildValidGstin("27ABCDE1234F1Z"),
                LegalName = "Seller Pvt Ltd",
                AddressLine1 = "1 GST Street",
                Location = "Mumbai",
                PostalCode = 400001,
                StateCode = "27"
            },
            Buyer = new GstInvoiceParty
            {
                Gstin = BuildValidGstin("29ABCDE1234F1Z"),
                LegalName = "Buyer Pvt Ltd",
                AddressLine1 = "2 Tax Road",
                Location = "Bengaluru",
                PostalCode = 560001,
                StateCode = "29"
            },
            Totals = new GstInvoiceTotals
            {
                AssessableValue = 1000m,
                CgstValue = 90m,
                SgstValue = 90m,
                IgstValue = 0m,
                TotalInvoiceValue = 1180m
            },
            Items =
            {
                new GstInvoiceItem
                {
                    SerialNumber = "1",
                    Description = "Consulting Service",
                    HsnCode = "998313",
                    IsService = true,
                    Quantity = 1m,
                    UnitPrice = 1000m,
                    TaxableAmount = 1000m,
                    TotalAmount = 1180m,
                    Tax = new GstInvoiceTax
                    {
                        GstRate = 18m,
                        CgstAmount = 90m,
                        SgstAmount = 90m,
                        IgstAmount = 0m
                    }
                }
            }
        };
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
    }
}
