namespace BuildGST.Abstractions.Interfaces;

public interface IGstApiProviderResolver
{
    IGstApiProvider Resolve(string? providerName = null);
}
