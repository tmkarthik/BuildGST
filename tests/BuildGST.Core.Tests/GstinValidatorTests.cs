using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstinValidatorTests
{
    [Fact]
    public void IsValid_ShouldReturnTrue_ForAValidGstin()
    {
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1Z");

        Assert.True(validator.IsValid(gstin));
        Assert.Equal(string.Empty, validator.GetValidationError(gstin));
    }

    [Fact]
    public void GetValidationError_ShouldReturnLengthError_ForShortGstin()
    {
        var validator = new GstinValidator();

        var error = validator.GetValidationError("27ABCDE1234F1");

        Assert.Equal("GSTIN must contain exactly 15 characters.", error);
        Assert.False(validator.IsValid("27ABCDE1234F1"));
    }

    [Fact]
    public void GetValidationError_ShouldReturnChecksumError_ForInvalidChecksum()
    {
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1Z");
        var invalidGstin = gstin.Substring(0, 14) + (gstin[14] == 'A' ? 'B' : 'A');

        var error = validator.GetValidationError(invalidGstin);

        Assert.Equal("GSTIN checksum validation failed.", error);
        Assert.False(validator.IsValid(invalidGstin));
    }

    [Fact]
    public void GetValidationError_ShouldReturnPanError_ForInvalidPanStructure()
    {
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("271BCDE1234F1Z");

        var error = validator.GetValidationError(gstin);

        Assert.Equal("GSTIN PAN structure is invalid.", error);
        Assert.False(validator.IsValid(gstin));
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
    }
}
