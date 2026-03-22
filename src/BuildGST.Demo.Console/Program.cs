using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace BuildGST.DemoApp;

internal static class Program
{
    private static async Task Main()
    {
        var sandboxTaxPayers = new Dictionary<string, GstTaxPayer>(StringComparer.OrdinalIgnoreCase);

        var services = new ServiceCollection();
        services.AddBuildGst(options =>
        {
            options.Provider = ProviderType.Sandbox;
        });
        services.AddSingleton<IReadOnlyDictionary<string, GstTaxPayer>>(sandboxTaxPayers);

        using var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IGstinValidator>();
        var lookupService = serviceProvider.GetRequiredService<IGstLookupService>();
        var invoiceGenerator = serviceProvider.GetRequiredService<IEInvoiceJsonGenerator>();
        var schemaValidator = serviceProvider.GetRequiredService<IInvoiceSchemaValidator>();

        WriteHeader("BuildGST Demo");
        Console.Write("Enter GSTIN: ");
        var input = Console.ReadLine() ?? string.Empty;

        var validationError = validator.GetValidationError(input);
        var normalizedGstin = input.Trim().ToUpperInvariant();
        var isValid = string.IsNullOrEmpty(validationError);

        WriteResult("Validation result", isValid ? "Valid GSTIN" : validationError);
        if (!isValid)
        {
            return;
        }

        SeedSandboxData(sandboxTaxPayers, normalizedGstin);

        var taxpayer = await lookupService.LookupAsync(normalizedGstin, CancellationToken.None);
        WriteResult("Taxpayer name", taxpayer.LegalName);
        WriteResult("Trade name", taxpayer.TradeName ?? "N/A");
        WriteResult("Registration status", taxpayer.RegistrationStatus ?? "N/A");

        var invoice = CreateSampleInvoice(normalizedGstin, taxpayer);
        var invoiceJson = await invoiceGenerator.GenerateAsync(invoice, CancellationToken.None);
        var schemaIsValid = schemaValidator.Validate(invoiceJson);
        var schemaErrors = schemaValidator.GetValidationErrors(invoiceJson);

        WriteResult("Schema validation result", schemaIsValid ? "Valid" : string.Join(Environment.NewLine, schemaErrors));
        WriteSection("Invoice JSON");
        Console.WriteLine(invoiceJson);
    }

    private static void SeedSandboxData(IDictionary<string, GstTaxPayer> sandboxTaxPayers, string gstin)
    {
        sandboxTaxPayers[gstin] = new GstTaxPayer
        {
            Gstin = gstin,
            LegalName = "Contoso Buyer Private Limited",
            TradeName = "Contoso Buyer",
            RegistrationStatus = "Active",
            TaxpayerType = "Regular",
            Address = new PostalAddress
            {
                BuildingNumber = "42",
                Street = "GST Avenue",
                Location = "Bengaluru",
                City = "Bengaluru",
                StateCode = gstin.Substring(0, 2),
                PostalCode = "560001"
            }
        };
    }

    private static GstInvoice CreateSampleInvoice(string buyerGstin, GstTaxPayer taxpayer)
    {
        return new GstInvoice
        {
            Metadata = new GstInvoiceMetadata
            {
                InvoiceNumber = "INV-2026-0001",
                InvoiceDate = "22/03/2026",
                DocumentType = "INV",
                SupplyType = "B2B"
            },
            Seller = new GstInvoiceParty
            {
                Gstin = BuildValidGstin("27ABCDE1234F1Z"),
                LegalName = "BuildGST Seller Private Limited",
                TradeName = "BuildGST Seller",
                AddressLine1 = "100 Tax Tower",
                Location = "Mumbai",
                PostalCode = 400001,
                StateCode = "27"
            },
            Buyer = new GstInvoiceParty
            {
                Gstin = buyerGstin,
                LegalName = taxpayer.LegalName,
                TradeName = taxpayer.TradeName,
                AddressLine1 = taxpayer.Address?.Street ?? "42 GST Avenue",
                Location = taxpayer.Address?.City ?? "Bengaluru",
                PostalCode = ParsePostalCode(taxpayer.Address?.PostalCode),
                StateCode = taxpayer.Address?.StateCode ?? buyerGstin.Substring(0, 2)
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
                    Description = "GST Utility Subscription",
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

    private static int ParsePostalCode(string? postalCode)
    {
        return int.TryParse(postalCode, out var parsed) ? parsed : 560001;
    }

    private static string BuildValidGstin(string prefix)
    {
        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var factor = 2;
        var sum = 0;

        for (var index = prefix.Length - 1; index >= 0; index--)
        {
            var codePoint = charset.IndexOf(char.ToUpperInvariant(prefix[index]));
            var addend = factor * codePoint;
            factor = factor == 2 ? 1 : 2;
            addend = (addend / charset.Length) + (addend % charset.Length);
            sum += addend;
        }

        var remainder = sum % charset.Length;
        var checkCodePoint = (charset.Length - remainder) % charset.Length;
        return prefix + charset[checkCodePoint];
    }

    private static void WriteHeader(string title)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine();
    }

    private static void WriteResult(string label, string value)
    {
        Console.WriteLine(label + ":");
        Console.WriteLine(value);
        Console.WriteLine();
    }

    private static void WriteSection(string title)
    {
        Console.WriteLine(title + ":");
        Console.WriteLine(new string('-', title.Length + 1));
    }
}
