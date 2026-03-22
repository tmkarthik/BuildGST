using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.DemoApp;

/// <summary>
/// Executes the interactive BuildGST console demo workflow.
/// </summary>
public sealed class DemoConsoleApplication
{
    private readonly IGstinValidator _validator;
    private readonly IGstLookupService _lookupService;
    private readonly IEInvoiceJsonGenerator _invoiceGenerator;
    private readonly IInvoiceSchemaValidator _schemaValidator;
    private readonly IDictionary<string, GstTaxPayer> _sandboxTaxPayers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoConsoleApplication"/> class.
    /// </summary>
    public DemoConsoleApplication(
        IGstinValidator validator,
        IGstLookupService lookupService,
        IEInvoiceJsonGenerator invoiceGenerator,
        IInvoiceSchemaValidator schemaValidator,
        IReadOnlyDictionary<string, GstTaxPayer> sandboxTaxPayers)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _invoiceGenerator = invoiceGenerator ?? throw new ArgumentNullException(nameof(invoiceGenerator));
        _schemaValidator = schemaValidator ?? throw new ArgumentNullException(nameof(schemaValidator));
        _sandboxTaxPayers = sandboxTaxPayers as IDictionary<string, GstTaxPayer>
            ?? throw new ArgumentException("Sandbox taxpayer store must support updates.", nameof(sandboxTaxPayers));
    }

    /// <summary>
    /// Runs the interactive console workflow.
    /// </summary>
    /// <param name="input">The input reader.</param>
    /// <param name="output">The output writer.</param>
    /// <param name="cancellationToken">A token used to cancel the workflow.</param>
    /// <returns>An exit code where 0 indicates success.</returns>
    public async Task<int> RunAsync(TextReader input, TextWriter output, CancellationToken cancellationToken = default)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        cancellationToken.ThrowIfCancellationRequested();

        await WriteHeaderAsync(output, "BuildGST Demo").ConfigureAwait(false);
        await output.WriteAsync("Enter GSTIN: ").ConfigureAwait(false);
        var enteredGstin = await input.ReadLineAsync().ConfigureAwait(false) ?? string.Empty;

        var validationError = _validator.GetValidationError(enteredGstin);
        var normalizedGstin = enteredGstin.Trim().ToUpperInvariant();

        await WriteResultAsync(output, "Validation result", string.IsNullOrEmpty(validationError) ? "Valid GSTIN" : validationError).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationError))
        {
            return 1;
        }

        SeedSandboxData(normalizedGstin);

        try
        {
            var taxpayer = await _lookupService.LookupAsync(normalizedGstin, cancellationToken).ConfigureAwait(false);
            await WriteResultAsync(output, "Taxpayer name", taxpayer.LegalName).ConfigureAwait(false);
            await WriteResultAsync(output, "Trade name", taxpayer.TradeName ?? "N/A").ConfigureAwait(false);
            await WriteResultAsync(output, "Registration status", taxpayer.RegistrationStatus ?? "N/A").ConfigureAwait(false);

            var invoice = CreateSampleInvoice(normalizedGstin, taxpayer);
            var invoiceJson = await _invoiceGenerator.GenerateAsync(invoice, cancellationToken).ConfigureAwait(false);
            var schemaIsValid = _schemaValidator.Validate(invoiceJson);
            var schemaErrors = _schemaValidator.GetValidationErrors(invoiceJson);

            await WriteResultAsync(
                output,
                "Schema validation result",
                schemaIsValid ? "Valid" : string.Join(Environment.NewLine, schemaErrors)).ConfigureAwait(false);

            await WriteSectionAsync(output, "Invoice JSON").ConfigureAwait(false);
            await output.WriteLineAsync(invoiceJson).ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            await WriteResultAsync(output, "Error", exception.Message).ConfigureAwait(false);
            return 1;
        }
    }

    private void SeedSandboxData(string gstin)
    {
        _sandboxTaxPayers[gstin] = new GstTaxPayer
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

    private static Task WriteHeaderAsync(TextWriter output, string title)
    {
        return output.WriteAsync(title + Environment.NewLine + new string('=', title.Length) + Environment.NewLine + Environment.NewLine);
    }

    private static Task WriteResultAsync(TextWriter output, string label, string value)
    {
        return output.WriteAsync(label + ":" + Environment.NewLine + value + Environment.NewLine + Environment.NewLine);
    }

    private static Task WriteSectionAsync(TextWriter output, string title)
    {
        return output.WriteAsync(title + ":" + Environment.NewLine + new string('-', title.Length + 1) + Environment.NewLine);
    }
}
