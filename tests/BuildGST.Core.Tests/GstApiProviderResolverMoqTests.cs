using System;
using System.Collections.Generic;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;
using BuildGST.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BuildGST.Core.Tests;

public sealed class GstApiProviderResolverMoqTests
{
    [Fact]
    public void Resolve_ShouldReturnThirdPartyProvider_WhenConfigured()
    {
        // Arrange
        var providers = new List<IGstApiProvider>
        {
            CreateProvider("government"),
            CreateProvider("third-party"),
            CreateProvider("sandbox")
        };

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(provider => provider.GetService(typeof(IEnumerable<IGstApiProvider>)))
            .Returns(providers);

        var options = Options.Create(new GstApiProviderOptions { Provider = ProviderType.ThirdParty });
        var resolver = new GstApiProviderResolver(serviceProvider.Object, options);

        // Act
        var provider = resolver.Resolve();

        // Assert
        Assert.Equal("third-party", provider.Name);
    }

    [Fact]
    public void Resolve_ShouldReturnSandboxProvider_WhenConfigured()
    {
        // Arrange
        var providers = new List<IGstApiProvider>
        {
            CreateProvider("government"),
            CreateProvider("third-party"),
            CreateProvider("sandbox")
        };

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(provider => provider.GetService(typeof(IEnumerable<IGstApiProvider>)))
            .Returns(providers);

        var options = Options.Create(new GstApiProviderOptions { Provider = ProviderType.Sandbox });
        var resolver = new GstApiProviderResolver(serviceProvider.Object, options);

        // Act
        var provider = resolver.Resolve();

        // Assert
        Assert.Equal("sandbox", provider.Name);
    }

    private static IGstApiProvider CreateProvider(string name)
    {
        var provider = new Mock<IGstApiProvider>();
        provider.SetupGet(value => value.Name).Returns(name);
        return provider.Object;
    }
}
