using System.Collections.Generic;
using BuildGST.Abstractions.Models;
using BuildGST.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace BuildGST.DemoApp;

internal static class Program
{
    private static async Task Main()
    {
        var sandboxTaxPayers = new Dictionary<string, GstTaxPayer>(StringComparer.OrdinalIgnoreCase);

        var services = new ServiceCollection();
        services.AddBuildGst(options =>
        {
            options.Provider = ProviderType.Sandbox;
        });
        services.AddSingleton<IReadOnlyDictionary<string, GstTaxPayer>>(sandboxTaxPayers);
        services.AddTransient<DemoConsoleApplication>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<DemoConsoleApplication>();
        var exitCode = await application.RunAsync(System.Console.In, System.Console.Out).ConfigureAwait(false);
        Environment.ExitCode = exitCode;
    }
}
