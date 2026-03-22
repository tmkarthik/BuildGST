using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

/// <summary>
/// Represents an in-memory GST provider intended for tests and local development.
/// </summary>
public sealed class MockGstApiProvider : IGstApiProvider
{
    private readonly IReadOnlyDictionary<string, GstTaxPayer> _taxPayers;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockGstApiProvider"/> class.
    /// </summary>
    /// <param name="taxPayers">The in-memory taxpayer records keyed by GSTIN.</param>
    public MockGstApiProvider(IReadOnlyDictionary<string, GstTaxPayer> taxPayers)
    {
        _taxPayers = taxPayers ?? throw new ArgumentNullException(nameof(taxPayers));
    }

    /// <inheritdoc />
    public string Name => "sandbox";

    /// <inheritdoc />
    public Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(gstin))
        {
            throw new ArgumentException("GSTIN is required.", nameof(gstin));
        }

        if (_taxPayers.TryGetValue(gstin, out var taxPayer))
        {
            return Task.FromResult(taxPayer);
        }

        throw new KeyNotFoundException($"No mock taxpayer was found for GSTIN '{gstin}'.");
    }
}
