using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

public interface IGstinValidator
{
    GstinValidationResult Validate(string gstin);
}
