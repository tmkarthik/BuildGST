using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.DemoApp;
using Moq;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class DemoConsoleApplicationTests
{
    [Fact]
    public async Task RunAsync_ShouldPrintFullSuccessFlow_ForValidGstin()
    {
        // Arrange
        const string gstin = "29ABCDE1234F1ZW";
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var lookupService = new Mock<IGstLookupService>(MockBehavior.Strict);
        var invoiceGenerator = new Mock<IEInvoiceJsonGenerator>(MockBehavior.Strict);
        var schemaValidator = new Mock<IInvoiceSchemaValidator>(MockBehavior.Strict);
        var sandboxData = new Dictionary<string, GstTaxPayer>(System.StringComparer.OrdinalIgnoreCase);
        var taxpayer = new GstTaxPayer
        {
            Gstin = gstin,
            LegalName = "Contoso Buyer Private Limited",
            TradeName = "Contoso Buyer",
            RegistrationStatus = "Active",
            Address = new PostalAddress
            {
                Street = "GST Avenue",
                City = "Bengaluru",
                StateCode = "29",
                PostalCode = "560001"
            }
        };

        validator.Setup(value => value.GetValidationError(gstin)).Returns(string.Empty);
        lookupService.Setup(value => value.LookupAsync(gstin, CancellationToken.None)).ReturnsAsync(taxpayer);
        invoiceGenerator.Setup(value => value.GenerateAsync(It.IsAny<GstInvoice>(), CancellationToken.None)).ReturnsAsync("{\"version\":\"1.1\"}");
        schemaValidator.Setup(value => value.Validate("{\"version\":\"1.1\"}")).Returns(true);
        schemaValidator.Setup(value => value.GetValidationErrors("{\"version\":\"1.1\"}")).Returns(System.Array.Empty<string>());

        var application = new DemoConsoleApplication(
            validator.Object,
            lookupService.Object,
            invoiceGenerator.Object,
            schemaValidator.Object,
            sandboxData);

        using var input = new StringReader(gstin + System.Environment.NewLine);
        using var output = new StringWriter();

        // Act
        var exitCode = await application.RunAsync(input, output, CancellationToken.None);
        var consoleOutput = output.ToString();

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("Enter GSTIN:", consoleOutput);
        Assert.Contains("Validation result:", consoleOutput);
        Assert.Contains("Taxpayer name:", consoleOutput);
        Assert.Contains("Invoice JSON:", consoleOutput);
        Assert.Contains("Schema validation result:", consoleOutput);
        Assert.Contains("Contoso Buyer Private Limited", consoleOutput);
        lookupService.VerifyAll();
        invoiceGenerator.VerifyAll();
        schemaValidator.VerifyAll();
    }

    [Fact]
    public async Task RunAsync_ShouldStopAfterValidationFailure_ForInvalidGstin()
    {
        // Arrange
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var lookupService = new Mock<IGstLookupService>(MockBehavior.Strict);
        var invoiceGenerator = new Mock<IEInvoiceJsonGenerator>(MockBehavior.Strict);
        var schemaValidator = new Mock<IInvoiceSchemaValidator>(MockBehavior.Strict);
        var sandboxData = new Dictionary<string, GstTaxPayer>(System.StringComparer.OrdinalIgnoreCase);

        validator.Setup(value => value.GetValidationError("INVALID")).Returns("GSTIN format is invalid.");

        var application = new DemoConsoleApplication(
            validator.Object,
            lookupService.Object,
            invoiceGenerator.Object,
            schemaValidator.Object,
            sandboxData);

        using var input = new StringReader("INVALID" + System.Environment.NewLine);
        using var output = new StringWriter();

        // Act
        var exitCode = await application.RunAsync(input, output, CancellationToken.None);
        var consoleOutput = output.ToString();

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Validation result:", consoleOutput);
        Assert.Contains("GSTIN format is invalid.", consoleOutput);
        lookupService.Verify(value => value.LookupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        invoiceGenerator.Verify(value => value.GenerateAsync(It.IsAny<GstInvoice>(), It.IsAny<CancellationToken>()), Times.Never);
        schemaValidator.Verify(value => value.Validate(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ShouldPrintError_WhenLookupFails()
    {
        // Arrange
        const string gstin = "29ABCDE1234F1ZW";
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var lookupService = new Mock<IGstLookupService>(MockBehavior.Strict);
        var invoiceGenerator = new Mock<IEInvoiceJsonGenerator>(MockBehavior.Strict);
        var schemaValidator = new Mock<IInvoiceSchemaValidator>(MockBehavior.Strict);
        var sandboxData = new Dictionary<string, GstTaxPayer>(System.StringComparer.OrdinalIgnoreCase);

        validator.Setup(value => value.GetValidationError(gstin)).Returns(string.Empty);
        lookupService.Setup(value => value.LookupAsync(gstin, CancellationToken.None)).ThrowsAsync(new System.InvalidOperationException("Lookup failed."));

        var application = new DemoConsoleApplication(
            validator.Object,
            lookupService.Object,
            invoiceGenerator.Object,
            schemaValidator.Object,
            sandboxData);

        using var input = new StringReader(gstin + System.Environment.NewLine);
        using var output = new StringWriter();

        // Act
        var exitCode = await application.RunAsync(input, output, CancellationToken.None);
        var consoleOutput = output.ToString();

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("Error:", consoleOutput);
        Assert.Contains("Lookup failed.", consoleOutput);
        invoiceGenerator.Verify(value => value.GenerateAsync(It.IsAny<GstInvoice>(), It.IsAny<CancellationToken>()), Times.Never);
        schemaValidator.Verify(value => value.Validate(It.IsAny<string>()), Times.Never);
    }
}
