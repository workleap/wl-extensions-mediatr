using System.Reflection;
using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Workleap.Extensions.MediatR;

public sealed class MediatorBuilder
{
    private readonly List<Action<MediatRServiceConfiguration>> _configurationActions = new();

    internal MediatorBuilder(IServiceCollection services, IEnumerable<Assembly> assemblies, Action<MediatRServiceConfiguration>? configure)
    {
        void RegisterAssemblies(MediatRServiceConfiguration configuration)
        {
            foreach (var assembly in assemblies)
            {
                configuration.RegisterServicesFromAssembly(assembly);
            }
        }

        this.Services = services;

        EnsureAddMediatorIsOnlyCalledOnce(services);

        // Store the configuration actions to be applied later
        this._configurationActions.Add(RegisterAssemblies);
        if (configure != null)
        {
            this._configurationActions.Add(configure);
        }

        // Register MediatR with the combined configuration
        services.AddMediatR(this.CreateConfigurationAction());
    }

    internal MediatorBuilder(IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration>? configure)
    {
        void RegisterAssembliesOfTypes(MediatRServiceConfiguration configuration)
        {
            foreach (var type in handlerAssemblyMarkerTypes)
            {
                configuration.RegisterServicesFromAssemblyContaining(type);
            }
        }

        this.Services = services;

        EnsureAddMediatorIsOnlyCalledOnce(services);

        // Store the configuration actions to be applied later
        this._configurationActions.Add(RegisterAssembliesOfTypes);
        if (configure != null)
        {
            this._configurationActions.Add(configure);
        }

        // Register MediatR with the combined configuration
        services.AddMediatR(this.CreateConfigurationAction());
    }

    public IServiceCollection Services { get; }

    /// <summary>
    /// Configures MediatR with the specified license key.
    /// Note: This method creates a new MediatR registration with the license key.
    /// Call this method immediately after AddMediator.
    /// </summary>
    /// <param name="licenseKey">The license key to use for MediatR.</param>
    /// <returns>The MediatorBuilder instance for method chaining.</returns>
    public MediatorBuilder WithLicenseKey(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            throw new ArgumentException("License key cannot be null or empty.", nameof(licenseKey));
        }

        // Add license key configuration to be applied during MediatR registration
        this._configurationActions.Add(config => config.LicenseKey = licenseKey);

        // Since MediatR is already registered, we need to re-register it with the license configuration
        this.ReregisterMediatRWithLicense();

        return this;
    }

    /// <summary>
    /// Configures MediatR with a license key from the specified environment variable.
    /// </summary>
    /// <param name="environmentVariable">The name of the environment variable containing the license key. Defaults to "MEDIATR_LICENSE_KEY".</param>
    /// <returns>The MediatorBuilder instance for method chaining.</returns>
    public MediatorBuilder WithLicenseKeyFromEnvironment(string environmentVariable = "MEDIATR_LICENSE_KEY")
    {
        var licenseKey = Environment.GetEnvironmentVariable(environmentVariable);

        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            return this.WithLicenseKey(licenseKey);
        }

        return this;
    }

    private void ReregisterMediatRWithLicense()
    {
        // Remove existing MediatR registrations
        var existingDescriptors = this.Services
            .Where(x => x.ServiceType == typeof(IMediator) ||
                       x.ServiceType == typeof(ISender) ||
                       x.ServiceType == typeof(IPublisher))
            .ToList();

        foreach (var descriptor in existingDescriptors)
        {
            this.Services.Remove(descriptor);
        }

        // Re-register MediatR with updated configuration including license
        this.Services.AddMediatR(this.CreateConfigurationAction());
    }

    private Action<MediatRServiceConfiguration> CreateConfigurationAction()
    {
        return configuration =>
        {
            ConfigureDefaultConfiguration(configuration);

            // Apply all stored configuration actions (including license key)
            foreach (var action in this._configurationActions)
            {
                action(configuration);
            }

            // TEMPORARY HACK UNTIL AUTOMATIC REGISTRATION OF PRE/POST PROCESSORS IS FIXED
            // See: https://github.com/jbogard/MediatR/pull/989#issuecomment-1883574379
            RegisterPreAndPostNonGenericClosedProcessors(configuration);

            // Restore the previous behavior of registering generic handlers from before MediatR 12.4.1
            // https://github.com/jbogard/MediatR/compare/v12.4.0...v12.4.1
            configuration.RegisterGenericHandlers = true;
        };
    }

    private static void EnsureAddMediatorIsOnlyCalledOnce(IServiceCollection services)
    {
        // If a service descriptor references one of our internal behaviors, it means we already executed our AddMediator method.
        // We prevent this because MediatR's "AddMediatR" method doesn't completely check for duplicate registration of behaviors
        // and might register duplicate behaviors if someone call this method multiple times
        // This will be addressed in https://github.com/jbogard/MediatR/pull/860
        if (services.Any(x => x.ImplementationType == typeof(RequestTracingBehavior<,>)))
        {
            throw new InvalidOperationException(nameof(ServiceCollectionExtensions.AddMediator) + " cannot be called multiple times");
        }
    }

    private static void ConfigureDefaultConfiguration(MediatRServiceConfiguration configuration)
    {
        // By default, register IMediator as a singleton, we don't want to create a new instance of Mediator every time
        // Request handlers are still registered as transient though
        configuration.Lifetime = ServiceLifetime.Singleton;

        // Register open singleton behaviors, invoked for any type of request
        // OpenTelemetry tracing first
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestTracingBehavior<,>), ServiceLifetime.Singleton));
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestTracingBehavior<,>), ServiceLifetime.Singleton));

        // Then logging, so the logs can be linked to the parent traces
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>), ServiceLifetime.Singleton));
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestLoggingBehavior<,>), ServiceLifetime.Singleton));

        // Then validation so errors can be recorded by tracing and logging
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>), ServiceLifetime.Singleton));
        configuration.BehaviorsToRegister.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>), ServiceLifetime.Singleton));
    }

    private static readonly PropertyInfo? AssembliesToRegisterPropertyInfo = typeof(MediatRServiceConfiguration).GetProperty(
        name: "AssembliesToRegister",
        bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null,
        returnType: typeof(List<Assembly>),
        types: Array.Empty<Type>(),
        modifiers: null);

    private static readonly MethodInfo? ConnectImplementationsToTypesClosingMethodInfo = typeof(ServiceRegistrar).GetMethod(
        name: "ConnectImplementationsToTypesClosing",
        bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
        binder: null,
        types: new[] { typeof(Type), typeof(IServiceCollection), typeof(IEnumerable<Assembly>), typeof(bool), typeof(MediatRServiceConfiguration), typeof(CancellationToken) },
        modifiers: null);

    private static void RegisterPreAndPostNonGenericClosedProcessors(MediatRServiceConfiguration configuration)
    {
        if (AssembliesToRegisterPropertyInfo == null || ConnectImplementationsToTypesClosingMethodInfo == null)
        {
            // If this happens, it means MediatR internals have changed. Maybe this hack isn't needed anymore?
            // In any case, our tests will reflect if this is still needed or not
            return;
        }

        // We basically execute the same code that worked in MediatR 12.0.1 when pre/post processors were automatically registered:
        // https://github.com/jbogard/MediatR/blob/v12.0.1/src/MediatR/Registration/ServiceRegistrar.cs#L21-L22
        var assembliesToRegister = (List<Assembly>)AssembliesToRegisterPropertyInfo.GetValue(configuration)!;

        // Populating "RequestPreProcessorsToRegister" will make MediatR register the pre-processor behavior that will invoke the processors:
        // https://github.com/jbogard/MediatR/blob/v12.2.0/src/MediatR/Registration/ServiceRegistrar.cs#L247-L251
        var preProcessorServiceDescriptors = new ServiceCollection();
        ConnectImplementationsToTypesClosingMethodInfo.Invoke(obj: null, parameters: new object?[]
        {
            typeof(IRequestPreProcessor<>), preProcessorServiceDescriptors, assembliesToRegister, true, configuration, CancellationToken.None,
        });
        configuration.RequestPreProcessorsToRegister.AddRange(preProcessorServiceDescriptors);

        // Same thing for the post-processors:
        // https://github.com/jbogard/MediatR/blob/v12.2.0/src/MediatR/Registration/ServiceRegistrar.cs#L253-L257
        var postProcessorServiceDescriptors = new ServiceCollection();
        configuration.RequestPostProcessorsToRegister.AddRange(postProcessorServiceDescriptors);
    }
}