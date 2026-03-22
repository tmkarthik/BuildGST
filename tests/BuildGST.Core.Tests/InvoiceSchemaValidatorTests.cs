using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class InvoiceSchemaValidatorTests
{
    [Fact]
    public async Task Validate_ShouldReturnTrue_ForValidInvoiceJson()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var validator = new InvoiceSchemaValidator();
        var json = await generator.GenerateAsync(CreateValidInvoice(), CancellationToken.None);

        var isValid = validator.Validate(json);
        var errors = validator.GetValidationErrors(json);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldReturnFalse_ForInvalidSchema()
    {
        var validator = new InvoiceSchemaValidator();
        var json = "{ \"version\": \"1.1\", \"seller\": { \"gstin\": \"27ABCDE1234F1Z5\", \"legalName\": \"Seller\", \"addressLine1\": \"1 GST Street\", \"location\": \"Mumbai\", \"postalCode\": \"bad\", \"stateCode\": \"27\" }, \"buyer\": { \"gstin\": \"29ABCDE1234F1Z2\", \"legalName\": \"Buyer\", \"addressLine1\": \"2 Tax Road\", \"location\": \"Bengaluru\", \"postalCode\": 560001, \"stateCode\": \"29\" }, \"invoiceMetadata\": { \"supplyType\": \"B2B\", \"documentType\": \"INV\", \"invoiceNumber\": \"INV-1\", \"invoiceDate\": \"22/03/2026\" }, \"itemList\": [{ \"serialNumber\": \"1\", \"description\": \"Service\", \"hsnCode\": \"998313\", \"isService\": \"Y\", \"quantity\": 1, \"unitPrice\": 1000, \"taxableAmount\": 1000, \"taxDetails\": { \"gstRate\": 18, \"cgstAmount\": 90, \"sgstAmount\": 90, \"igstAmount\": 0 }, \"totalAmount\": 1180 }], \"totals\": { \"assessableValue\": 1000, \"cgstValue\": 90, \"sgstValue\": 90, \"igstValue\": 0, \"totalInvoiceValue\": 1180 } }";

        var isValid = validator.Validate(json);
        var errors = validator.GetValidationErrors(json);

        Assert.False(isValid);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task Validate_ShouldReturnFalse_WhenRequiredFieldIsMissing()
    {
        var generator = new EInvoiceJsonGenerator(new GstinValidator());
        var validator = new InvoiceSchemaValidator();
        var json = await generator.GenerateAsync(CreateValidInvoice(), CancellationToken.None);
        var invalidJson = json.Replace("\"legalName\": \"Buyer Pvt Ltd\",", string.Empty);

        var isValid = validator.Validate(invalidJson);
        var errors = validator.GetValidationErrors(invalidJson);

        Assert.False(isValid);
        Assert.NotEmpty(errors);
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
