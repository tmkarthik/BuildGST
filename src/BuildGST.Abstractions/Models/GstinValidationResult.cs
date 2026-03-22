namespace BuildGST.Abstractions.Models;

public sealed class GstinValidationResult
{
    public GstinValidationResult(bool isValid, string normalizedGstin, string? errorMessage = null)
    {
        IsValid = isValid;
        NormalizedGstin = normalizedGstin;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public string NormalizedGstin { get; }

    public string? ErrorMessage { get; }
}
