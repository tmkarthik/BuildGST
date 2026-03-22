using BuildGST.Core.Validation;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstinValidatorTests
{
    [Fact]
    public void Validate_ShouldAcceptAWellFormedGstin()
    {
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1Z");

        var result = validator.Validate(gstin);

        Assert.True(result.IsValid);
        Assert.Equal(gstin, result.NormalizedGstin);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_ShouldRejectGstinWithInvalidChecksum()
    {
        var validator = new GstinValidator();
        var gstin = BuildValidGstin("27ABCDE1234F1Z");
        var invalidGstin = gstin.Substring(0, 14) + (gstin[14] == 'A' ? 'B' : 'A');

        var result = validator.Validate(invalidGstin);

        Assert.False(result.IsValid);
        Assert.Equal("GSTIN checksum validation failed.", result.ErrorMessage);
    }

    private static string BuildValidGstin(string prefix)
    {
        return prefix + GstinValidator.CalculateChecksum(prefix);
    }
}
