using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

/// <summary>
/// Generates GST e-invoice JSON payloads.
/// </summary>
public interface IEInvoiceJsonGenerator
{
    /// <summary>
    /// Generates a GSTN-compatible JSON document for the supplied invoice.
    /// </summary>
    /// <param name="invoice">The invoice to serialize.</param>
    /// <param name="token">A token that can cancel the operation.</param>
    /// <returns>The generated JSON payload.</returns>
    Task<string> GenerateAsync(GstInvoice invoice, CancellationToken token = default);
}
