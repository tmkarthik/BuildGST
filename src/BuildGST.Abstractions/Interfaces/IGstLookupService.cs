using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Models;

namespace BuildGST.Abstractions.Interfaces;

public interface IGstLookupService
{
    Task<GstLookupResponse> LookupAsync(GstLookupRequest request, CancellationToken cancellationToken = default);
}
