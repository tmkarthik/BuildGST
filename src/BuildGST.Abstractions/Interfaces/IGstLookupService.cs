using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Provides GST taxpayer lookup operations.
/// </summary>
public interface IGstLookupService
{
    /// <summary>
    /// Looks up a GST taxpayer for the specified GSTIN.
    /// </summary>
    /// <param name="gstin">The GSTIN to look up.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The taxpayer details returned by the resolved provider.</returns>
    Task<GstTaxPayer> LookupAsync(string gstin, CancellationToken cancellationToken = default);
}
