using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Workleap.Extensions.MediatR.Tests;

public sealed class LicenseTest
{
    [Fact]
    public void WithLicenseKey_Should_Not_Throw()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // This should work without throwing
        services.AddMediator(typeof(LicenseTest).Assembly)
            .WithLicenseKey("test-license-key");

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void WithLicenseKeyFromEnvironment_Should_Not_Throw_When_Env_Var_Missing()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // This should work even if environment variable doesn't exist
        services.AddMediator(typeof(LicenseTest).Assembly)
            .WithLicenseKeyFromEnvironment("NON_EXISTENT_VAR");

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        Assert.NotNull(mediator);
    }
}