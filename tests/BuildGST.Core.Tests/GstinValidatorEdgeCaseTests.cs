using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstinValidatorEdgeCaseTests
{
    [Fact]
    public void GetValidationError_ShouldReturnStateCodeError_ForInvalidStateCode()
    {
        // Arrange
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("00ABCDE1234F1Z");

        // Act
        var error = validator.GetValidationError(gstin);

        // Assert
        Assert.Equal("GSTIN state code is invalid.", error);
    }

    [Fact]
    public void GetValidationError_ShouldReturnEntityCodeError_ForInvalidEntityCode()
    {
        // Arrange
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F0Z");

        // Act
        var error = validator.GetValidationError(gstin);

        // Assert
        Assert.Equal("GSTIN entity code is invalid.", error);
    }

    [Fact]
    public void GetValidationError_ShouldReturnZCharacterError_WhenFourteenthCharacterIsInvalid()
    {
        // Arrange
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1A");

        // Act
        var error = validator.GetValidationError(gstin);

        // Assert
        Assert.Equal("GSTIN 14th character must be 'Z'.", error);
    }

    [Fact]
    public void IsValid_ShouldAcceptTrimmedLowerCaseInput_WhenChecksumMatches()
    {
        // Arrange
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1Z").ToLowerInvariant();

        // Act
        var isValid = validator.IsValid("  " + gstin + "  ");

        // Assert
        Assert.True(isValid);
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
    }
}
