﻿namespace GSoft.Extensions.MediatR.Analyzers.Tests;

public sealed class NamingConventionAnalyzerTests : BaseAnalyzerTests<NamingConventionAnalyzer>
{
    [Fact]
    public async Task Request_Ending_With_Command_Returns_No_Diagnostic()
    {
        const string source = "public class MyCommand : IRequest { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Request_Ending_With_Query_Returns_No_Diagnostic()
    {
        const string source = "public class MyQuery : IRequest<string> { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task StreamRequest_Ending_With_StreamQuery_Returns_No_Diagnostic()
    {
        const string source = "public class MyStreamQuery : IStreamRequest<string> { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Notification_Ending_With_Notification_Returns_No_Diagnostic()
    {
        const string source = "public class MyNotification : INotification { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Notification_Ending_With_Event_Returns_No_Diagnostic()
    {
        const string source = "public class MyEvent : INotification { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Request_Not_Ending_With_Command_Or_Query_Returns_One_Diagnostic()
    {
        const string source = "public class MyClass : IRequest<string> { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseCommandOrQuerySuffixRule, Assert.Single(diagnostics).Descriptor);
    }

    [Fact]
    public async Task StreamRequest_Not_Ending_With_StreamQuery_Returns_One_Diagnostic()
    {
        const string source = "public class MyClass : IStreamRequest<string> { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseStreamQuerySuffixRule, Assert.Single(diagnostics).Descriptor);
    }

    [Fact]
    public async Task Notification_Not_Ending_With_Notification_Or_Event_Returns_One_Diagnostic()
    {
        const string source = "public class MyClass : INotification { }";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseNotificationOrEventSuffixRule, Assert.Single(diagnostics).Descriptor);
    }

    [Fact]
    public async Task RequestHandler_Ending_With_CommandHandler_Returns_No_Diagnostic()
    {
        const string source = @"
public class MyCommand : IRequest { }

internal class MyCommandHandler : IRequestHandler<MyCommand>
{
    public Task Handle(MyCommand command, CancellationToken cancellationToken) => Task.CompletedTask;
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RequestHandler_Ending_With_QueryHandler_Returns_No_Diagnostic()
    {
        const string source = @"
public class MyQuery : IRequest<string> { }

internal class MyQueryHandler : IRequestHandler<MyQuery, string>
{
    public Task<string> Handle(MyQuery query, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task StreamRequestHandler_Ending_With_StreamQueryHandler_Returns_No_Diagnostic()
    {
        const string source = @"
public class MyStreamQuery : IStreamRequest<string> { }

internal class MyStreamQueryHandler : IStreamRequestHandler<MyStreamQuery, string>
{
    public async IAsyncEnumerable<string> Handle(MyStreamQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield break;
    }
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task NotificationHandler_Ending_With_NotificationHandler_Returns_No_Diagnostic()
    {
        const string source = @"
public class MyNotification : INotification { }

internal class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task NotificationHandler_Ending_With_EventHandler_Returns_No_Diagnostic()
    {
        const string source = @"
public class MyEvent : INotification { }

internal class MyEventHandler : INotificationHandler<MyEvent>
{
    public Task Handle(MyEvent evt, CancellationToken cancellationToken) => Task.CompletedTask;
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RequestHandler_Not_Ending_With_CommandHandler_Or_QueryHandler_Returns_One_Diagnostic()
    {
        const string source = @"
public class MyCommand : IRequest { }

internal class MyRequestHandler : IRequestHandler<MyCommand>
{
    public Task Handle(MyCommand command, CancellationToken cancellationToken) => Task.CompletedTask;
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseCommandHandlerOrQueryHandlerSuffixRule, Assert.Single(diagnostics).Descriptor);
    }

    [Fact]
    public async Task StreamRequestHandler_Not_Ending_With_StreamQueryHandler_Returns_One_Diagnostic()
    {
        const string source = @"
public class MyStreamQuery : IStreamRequest<string> { }

internal class MyStreamRequestHandler : IStreamRequestHandler<MyStreamQuery, string>
{
    public async IAsyncEnumerable<string> Handle(MyStreamQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield break;
    }
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseStreamQueryHandlerSuffixRule, Assert.Single(diagnostics).Descriptor);
    }

    [Fact]
    public async Task NotificationHandler_Not_Ending_With_NotificationHandler_Or_EventHandler_Returns_One_Diagnostic()
    {
        const string source = @"
public class MyNotification : INotification { }

internal class SomethingHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
}";
        var diagnostics = await this.Helper.WithSourceFile("Program.cs", source).Compile();
        Assert.Same(NamingConventionAnalyzer.UseNotificationHandlerOrEventHandlerSuffixRule, Assert.Single(diagnostics).Descriptor);
    }
}