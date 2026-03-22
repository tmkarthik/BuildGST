using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Represents a GST API provider capable of retrieving taxpayer details for a GSTIN.
/// </summary>
public interface IGstApiProvider
{
    /// <summary>
    /// Gets the provider name used for configuration and resolution.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves taxpayer details for the specified GSTIN.
    /// </summary>
    /// <param name="gstin">The GSTIN to query.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that resolves to the taxpayer details.</returns>
    Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken);
}
