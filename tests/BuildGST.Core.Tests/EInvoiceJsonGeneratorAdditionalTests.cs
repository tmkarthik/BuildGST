using System;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class EInvoiceJsonGeneratorAdditionalTests
{
    [Fact]
    public async Task GenerateAsync_ShouldThrowOperationCanceledException_WhenTokenIsCanceled()
    {
        // Arrange
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var action = () => generator.GenerateAsync(invoice, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(action);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowInvalidOperationException_WhenTotalsDoNotMatch()
    {
        // Arrange
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();
        invoice.Totals.TotalInvoiceValue = 1000m;

        // Act
        var action = () => generator.GenerateAsync(invoice, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal("Invoice totals validation failed.", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrowArgumentException_WhenSellerGstinIsInvalid()
    {
        // Arrange
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var invoice = CreateValidInvoice();
        invoice.Seller.Gstin = "INVALID";

        // Act
        var action = () => generator.GenerateAsync(invoice, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Contains("Seller GSTIN is invalid.", exception.Message);
    }

    private static GstInvoice CreateValidInvoice()
    {
        return new GstInvoice
        {
            Metadata = new GstInvoiceMetadata
            {
                InvoiceNumber = "INV-2001",
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
