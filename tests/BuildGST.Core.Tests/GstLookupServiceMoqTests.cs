using System;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Exceptions;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using Moq;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstLookupServiceMoqTests
{
    [Fact]
    public async Task LookupAsync_ShouldReturnTaxPayer_WhenDependenciesSucceed()
    {
        // Arrange
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var resolver = new Mock<IGstApiProviderResolver>(MockBehavior.Strict);
        var provider = new Mock<IGstApiProvider>(MockBehavior.Strict);
        var logger = new Mock<IGstLookupLogger>(MockBehavior.Strict);
        var cancellationToken = new CancellationTokenSource().Token;
        var normalizedGstin = "27ABCDE1234F1Z5";
        var taxpayer = new GstTaxPayer { Gstin = normalizedGstin, LegalName = "Mock Taxpayer" };
        const string providerName = "government";

        validator.Setup(value => value.GetValidationError("27abcde1234f1z5")).Returns(string.Empty);
        resolver.Setup(value => value.Resolve()).Returns(provider.Object);
        provider.SetupGet(value => value.Name).Returns(providerName);
        logger.Setup(value => value.LookupStarted(normalizedGstin));
        provider.Setup(value => value.GetTaxPayerAsync(normalizedGstin, cancellationToken)).ReturnsAsync(taxpayer);
        logger.Setup(value => value.LookupSucceeded(normalizedGstin, providerName));

        var service = new GstLookupService(validator.Object, resolver.Object, new[] { logger.Object });

        // Act
        var result = await service.LookupAsync("27abcde1234f1z5", cancellationToken);

        // Assert
        Assert.Same(taxpayer, result);
        validator.VerifyAll();
        resolver.VerifyAll();
        provider.VerifyAll();
        logger.VerifyAll();
    }

    [Fact]
    public async Task LookupAsync_ShouldRejectInvalidInput_WithoutResolvingProvider()
    {
        // Arrange
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var resolver = new Mock<IGstApiProviderResolver>(MockBehavior.Strict);
        var logger = new Mock<IGstLookupLogger>(MockBehavior.Strict);

        validator.Setup(value => value.GetValidationError("BAD")).Returns("Invalid GST.");
        logger.Setup(value => value.ValidationFailed("BAD", "Invalid GST."));

        var service = new GstLookupService(validator.Object, resolver.Object, new[] { logger.Object });

        // Act
        var action = () => service.LookupAsync("BAD", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidGstinException>(action);
        resolver.Verify(value => value.Resolve(), Times.Never);
        validator.VerifyAll();
        logger.VerifyAll();
    }

    [Fact]
    public async Task LookupAsync_ShouldThrowOperationCanceledException_WhenTokenIsCanceled()
    {
        // Arrange
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var resolver = new Mock<IGstApiProviderResolver>(MockBehavior.Strict);
        var service = new GstLookupService(validator.Object, resolver.Object);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var action = () => service.LookupAsync("27ABCDE1234F1Z5", cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(action);
        validator.Verify(value => value.GetValidationError(It.IsAny<string>()), Times.Never);
        resolver.Verify(value => value.Resolve(), Times.Never);
    }

    [Fact]
    public async Task LookupAsync_ShouldWrapApiFailure()
    {
        // Arrange
        var validator = new Mock<IGstinValidator>(MockBehavior.Strict);
        var resolver = new Mock<IGstApiProviderResolver>(MockBehavior.Strict);
        var provider = new Mock<IGstApiProvider>(MockBehavior.Strict);
        var logger = new Mock<IGstLookupLogger>(MockBehavior.Strict);
        var normalizedGstin = "27ABCDE1234F1Z5";
        var apiException = new GstApiException("government", "Failure", 500, true);

        validator.Setup(value => value.GetValidationError(normalizedGstin)).Returns(string.Empty);
        resolver.Setup(value => value.Resolve()).Returns(provider.Object);
        logger.Setup(value => value.LookupStarted(normalizedGstin));
        provider.Setup(value => value.GetTaxPayerAsync(normalizedGstin, CancellationToken.None)).ThrowsAsync(apiException);
        logger.Setup(value => value.LookupFailed(normalizedGstin, apiException));

        var service = new GstLookupService(validator.Object, resolver.Object, new[] { logger.Object });

        // Act
        var action = () => service.LookupAsync(normalizedGstin, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<GstLookupFailedException>(action);
        Assert.Same(apiException, exception.InnerException);
        validator.VerifyAll();
        resolver.VerifyAll();
        provider.VerifyAll();
        logger.VerifyAll();
    }
}
