﻿using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Workleap.Extensions.MediatR.Analyzers.Tests;

public abstract class BaseAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private const string CSharp10GlobalUsings = @"
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
";

    private const string MediatRGlobalUsings = @"
global using Microsoft.Extensions.DependencyInjection;
global using System.Runtime.CompilerServices;
global using MediatR;";

    private const string SourceFileName = "Program.cs";

    protected BaseAnalyzerTest()
    {
        this.TestState.Sources.Add(CSharp10GlobalUsings);
        this.TestState.Sources.Add(MediatRGlobalUsings);

        this.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

        // Reference "Microsoft.Extensions.DependencyInjection" assembly
        this.TestState.AdditionalReferences.Add(typeof(IServiceCollection).Assembly);

        // Reference "MediatR" assembly
        this.TestState.AdditionalReferences.Add(typeof(IMediator).Assembly);

        // Reference "MediatR.Contracts" assembly
        this.TestState.AdditionalReferences.Add(typeof(IRequest).Assembly);

        // Reference "Workleap.Extensions.MediatR" assembly (our assembly)
        this.TestState.AdditionalReferences.Add(typeof(MediatorBuilder).Assembly);
    }

    protected override CompilationOptions CreateCompilationOptions()
        => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false);

    protected override ParseOptions CreateParseOptions()
        => new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.Diagnose);

    public BaseAnalyzerTest<TAnalyzer> WithExpectedDiagnostic(DiagnosticDescriptor descriptor, int startLine, int startColumn, int endLine, int endColumn)
    {
        this.TestState.ExpectedDiagnostics.Add(new DiagnosticResult(descriptor).WithSpan(SourceFileName, startLine, startColumn, endLine, endColumn));
        return this;
    }

    public BaseAnalyzerTest<TAnalyzer> WithExpectedDiagnostic(DiagnosticDescriptor descriptor, int locationIndex)
    {
        this.TestState.ExpectedDiagnostics.Add(new DiagnosticResult(descriptor).WithLocation(locationIndex));
        return this;
    }

    public BaseAnalyzerTest<TAnalyzer> WithDisabledDiagnostic(DiagnosticDescriptor descriptor)
    {
        this.DisabledDiagnostics.Add(descriptor.Id);
        return this;
    }

    protected BaseAnalyzerTest<TAnalyzer> WithSourceCode(string sourceCode)
    {
        this.TestState.Sources.Add((SourceFileName, sourceCode));
        return this;
    }
}