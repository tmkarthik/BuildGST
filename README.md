# BuildGST

Reusable GST utility class library designed in a clean, provider-based architecture for .NET Standard 2.0.

## Project structure

```text
BuildGST.sln
src/
  BuildGST.Abstractions/        # Interfaces, contracts, DTOs, options
  BuildGST.Core/                # Validation, orchestration, e-invoice generation
  BuildGST.Http/                # HTTP provider base classes and default configurable provider
  BuildGST.DependencyInjection/ # IServiceCollection registration helpers
tests/
  BuildGST.Core.Tests/          # Unit tests for validation and service behavior
```

## Key interfaces

- `IGstinValidator`: Validates GSTIN format and checksum.
- `IGstLookupService`: Async entry point for GST lookup workflows.
- `IGstApiProvider`: Provider contract for API-specific GST lookup implementations.
- `IGstApiProviderResolver`: Resolves the configured provider by name.
- `IEInvoiceJsonGenerator`: Produces GST e-invoice JSON payloads.

## Design patterns used

- Strategy: Each `IGstApiProvider` encapsulates one provider implementation.
- Factory/Resolver: `GstApiProviderResolver` selects the active provider at runtime.
- Dependency Injection: `AddBuildGst` wires validators, services, and providers.
- Template Method: `HttpGstApiProviderBase` centralizes HTTP flow while allowing provider-specific request/response behavior.
- Clean Architecture layering: Contracts in `Abstractions`, policies in `Core`, infrastructure in `Http`, composition in `DependencyInjection`.

## Notes

- The default HTTP provider is intentionally generic and configurable.
- Real provider-specific response mapping can be added by implementing `IGstApiProvider` or inheriting `HttpGstApiProviderBase`.
- The library uses task-based async APIs and is ready for unit testing through interface boundaries.
