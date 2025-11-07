using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Workleap.Extensions.MediatR.Tests;
public sealed class HostBuilderTests
{
    [Fact]
    public async Task SetLicenseFromConfiguration()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = false;
        });

        builder.Services.AddMediator(typeof(HostBuilderTests).Assembly);
        builder.Configuration.AddInMemoryCollection([KeyValuePair.Create<string, string?>("MEDIATR_LICENSE_KEY", "License")]);

        // The license is check in the constructor of the Mediator.
        // We want to ensure that the license is set before the Mediator is created.
        // This is done by using a custom Mediator that throws an exception in the constructor.
        builder.Services.AddSingleton<IMediator, CustomMediator>();
        builder.Services.AddSingleton<Mediator, CustomMediator>();

        await using var app = builder.Build();
        var task = app.RunAsync();

        var configuration = app.Services.GetRequiredService<MediatRServiceConfiguration>();
        Assert.Equal("License", configuration.LicenseKey);
    }

    private sealed class CustomMediator : Mediator
    {
        public CustomMediator(IServiceProvider serviceProvider, INotificationPublisher publisher) : base(serviceProvider, publisher)
        {
            throw new UnreachableException();
        }
    }
}
