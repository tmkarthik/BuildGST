using System.Collections.Generic;

namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Validates invoice JSON payloads against the GST invoice schema.
/// </summary>
public interface IInvoiceSchemaValidator
{
    /// <summary>
    /// Validates the supplied invoice JSON payload.
    /// </summary>
    /// <param name="json">The invoice JSON payload.</param>
    /// <returns><see langword="true"/> when the payload matches the schema; otherwise, <see langword="false"/>.</returns>
    bool Validate(string json);

    /// <summary>
    /// Gets the schema validation errors for the supplied invoice JSON payload.
    /// </summary>
    /// <param name="json">The invoice JSON payload.</param>
    /// <returns>The validation errors. An empty collection indicates a valid payload.</returns>
    IReadOnlyCollection<string> GetValidationErrors(string json);
}
