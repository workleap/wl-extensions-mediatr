﻿using System.Collections.Immutable;
using GSoft.Extensions.MediatR.Analyzers.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GSoft.Extensions.MediatR.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterUsageAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UseGenericParameterRule = new DiagnosticDescriptor(
        id: RuleIdentifiers.UseGenericParameter,
        title: "Use generic method instead",
        messageFormat: "Use generic method instead",
        category: RuleCategories.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: RuleIdentifiers.HelpUri);

    internal static readonly DiagnosticDescriptor ProvideCancellationTokenRule = new DiagnosticDescriptor(
        id: RuleIdentifiers.ProvideCancellationToken,
        title: "Provide a cancellation token",
        messageFormat: "Provide a cancellation token",
        category: RuleCategories.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: RuleIdentifiers.HelpUri);

    internal static readonly DiagnosticDescriptor UseMethodEndingWithAsyncRule = new DiagnosticDescriptor(
        id: RuleIdentifiers.UseMethodEndingWithAsync,
        title: "Use method ending with 'Async' instead",
        messageFormat: "Use method ending with 'Async' instead",
        category: RuleCategories.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: RuleIdentifiers.HelpUri);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        UseGenericParameterRule,
        ProvideCancellationTokenRule,
        UseMethodEndingWithAsyncRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(OnCompilationStarted);
    }

    private static void OnCompilationStarted(CompilationStartAnalysisContext context)
    {
        var analyzer = new AnalyzerImplementation(context.Compilation);
        if (analyzer.IsValid)
        {
            context.RegisterOperationAction(analyzer.AnalyzeOperationInvocation, OperationKind.Invocation);
        }
    }

    private sealed class AnalyzerImplementation
    {
        private static readonly HashSet<string> MediatorMethodNames = new HashSet<string>(StringComparer.Ordinal)
        {
            KnownSymbolNames.SendMethod,
            KnownSymbolNames.PublishMethod,
            KnownSymbolNames.CreateStreamMethod,
        };

        private static readonly HashSet<string> MediatorMethodNamesSupportingAsyncSuffix = new HashSet<string>(StringComparer.Ordinal)
        {
            KnownSymbolNames.SendMethod,
            KnownSymbolNames.PublishMethod,
        };

        private readonly HashSet<INamedTypeSymbol> _mediatorTypes;

        public AnalyzerImplementation(Compilation compilation)
        {
            this._mediatorTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            if (compilation.GetBestTypeByMetadataName(KnownSymbolNames.MediatorClass, KnownSymbolNames.MediatRAssembly) is { } mediatorType)
            {
                this._mediatorTypes.Add(mediatorType);
            }

            if (compilation.GetBestTypeByMetadataName(KnownSymbolNames.MediatorInterface, KnownSymbolNames.MediatRAssembly) is { } mediatorInterfaceType)
            {
                this._mediatorTypes.Add(mediatorInterfaceType);
            }

            if (compilation.GetBestTypeByMetadataName(KnownSymbolNames.SenderInterface, KnownSymbolNames.MediatRAssembly) is { } senderInterfaceType)
            {
                this._mediatorTypes.Add(senderInterfaceType);
            }

            if (compilation.GetBestTypeByMetadataName(KnownSymbolNames.PublisherInterface, KnownSymbolNames.MediatRAssembly) is { } publisherInterfaceType)
            {
                this._mediatorTypes.Add(publisherInterfaceType);
            }
        }

        public bool IsValid => this._mediatorTypes.Count == 4;

        public void AnalyzeOperationInvocation(OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation operation)
            {
                return;
            }

            if (!this.IsMediatorMethod(operation))
            {
                return;
            }

            if (MediatorMethodNamesSupportingAsyncSuffix.Contains(operation.TargetMethod.Name))
            {
                context.ReportDiagnostic(UseMethodEndingWithAsyncRule, operation);
            }

            if (!operation.TargetMethod.IsGenericMethod)
            {
                context.ReportDiagnostic(UseGenericParameterRule, operation);
            }

            if (IsDefaultCancellationTokenParameter(operation))
            {
                context.ReportDiagnostic(ProvideCancellationTokenRule, operation);
            }
        }

        private bool IsMediatorMethod(IInvocationOperation operation)
        {
            return operation.TargetMethod.Parameters.Length == 2
                && this._mediatorTypes.Contains(operation.TargetMethod.ContainingType)
                && MediatorMethodNames.Contains(operation.TargetMethod.Name);
        }

        private static bool IsDefaultCancellationTokenParameter(IInvocationOperation operation)
        {
            return operation.Arguments.Length == 2 && operation.Arguments[1].ArgumentKind == ArgumentKind.DefaultValue;
        }
    }
}