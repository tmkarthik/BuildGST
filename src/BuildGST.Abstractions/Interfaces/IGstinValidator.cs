namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Provides GSTIN validation operations.
/// </summary>
public interface IGstinValidator
{
    /// <summary>
    /// Determines whether the supplied GSTIN is valid.
    /// </summary>
    /// <param name="gstin">The GSTIN to validate.</param>
    /// <returns><see langword="true"/> when the GSTIN is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(string gstin);

    /// <summary>
    /// Gets the validation error for the supplied GSTIN.
    /// </summary>
    /// <param name="gstin">The GSTIN to validate.</param>
    /// <returns>An error message when validation fails; otherwise an empty string.</returns>
    string GetValidationError(string gstin);
}
