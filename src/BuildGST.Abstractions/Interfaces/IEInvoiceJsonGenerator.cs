using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

public interface IEInvoiceJsonGenerator
{
    string Generate(EInvoiceDocument document);
}
