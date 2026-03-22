using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Core.Services;

public sealed class EInvoiceJsonGenerator : IEInvoiceJsonGenerator
{
    public string Generate(EInvoiceDocument document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (string.IsNullOrWhiteSpace(document.DocumentDetails.Number))
        {
            throw new ArgumentException("Document number is required.", nameof(document));
        }

        if (string.IsNullOrWhiteSpace(document.SellerDetails.Gstin))
        {
            throw new ArgumentException("Seller GSTIN is required.", nameof(document));
        }

        if (string.IsNullOrWhiteSpace(document.BuyerDetails.Gstin))
        {
            throw new ArgumentException("Buyer GSTIN is required.", nameof(document));
        }

        if (document.Items == null || document.Items.Count == 0)
        {
            throw new ArgumentException("At least one invoice item is required.", nameof(document));
        }

        var serializer = new DataContractJsonSerializer(typeof(EInvoiceDocument));

        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, document);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
