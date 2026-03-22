using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class EInvoiceJsonGeneratorTests
{
    [Fact]
    public void Generate_ShouldProduceExpectedJsonShape()
    {
        var generator = new EInvoiceJsonGenerator();
        var document = new EInvoiceDocument
        {
            DocumentDetails = new DocumentDetails
            {
                Number = "INV-1001",
                Date = "22/03/2026"
            },
            SellerDetails = new PartyDetails
            {
                Gstin = "27ABCDE1234F1Z5",
                LegalName = "Seller Pvt Ltd",
                AddressLine1 = "1 GST Street",
                Location = "Mumbai",
                PostalCode = 400001,
                StateCode = "27"
            },
            BuyerDetails = new PartyDetails
            {
                Gstin = "29ABCDE1234F1Z1",
                LegalName = "Buyer Pvt Ltd",
                AddressLine1 = "2 Tax Road",
                Location = "Bengaluru",
                PostalCode = 560001,
                StateCode = "29"
            },
            ValueDetails = new InvoiceValueDetails
            {
                AssessableValue = 1000m,
                CgstValue = 90m,
                SgstValue = 90m,
                TotalInvoiceValue = 1180m
            }
        };

        document.Items.Add(new InvoiceItem
        {
            SerialNumber = "1",
            Description = "Consulting Service",
            IsService = "Y",
            HsnCode = "998313",
            Quantity = 1,
            UnitPrice = 1000m,
            TotalAmount = 1000m,
            GstRate = 18m,
            AssessableAmount = 1000m
        });

        var json = generator.Generate(document);

        Assert.Contains("\"Version\":\"1.1\"", json);
        Assert.Contains("\"DocDtls\"", json);
        Assert.Contains("\"ItemList\"", json);
        Assert.Contains("\"SellerDtls\"", json);
    }
}
