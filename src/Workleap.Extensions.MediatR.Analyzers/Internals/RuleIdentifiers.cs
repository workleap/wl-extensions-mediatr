namespace Workleap.Extensions.MediatR.Analyzers.Internals;

internal static class RuleIdentifiers
{
    public const string HelpUri = "https://github.com/workleap/wl-extensions-mediatr";

    // DO NOT change the identifier of existing rules.
    // Projects can customize the severity level of analysis rules using a .editorconfig file.
    public const string UseCommandOrQuerySuffix = "GMDTR01";
    public const string UseCommandHandlerOrQueryHandlerSuffix = "GMDTR02";
    public const string UseStreamQuerySuffix = "GMDTR03";
    public const string UseStreamQueryHandlerSuffix = "GMDTR04";
    public const string UseNotificationOrEventSuffix = "GMDTR05";
    public const string UseNotificationHandlerOrEventHandlerSuffix = "GMDTR06";
    public const string UseGenericParameter = "GMDTR07";
    public const string ProvideCancellationToken = "GMDTR08";
    public const string RequestHandlersShouldNotCallHandler = "GMDTR09";
    public const string HandlersShouldNotBePublic = "GMDTR10";
    public const string UseAddMediatorExtensionMethod = "GMDTR11";
    public const string UseMethodEndingWithAsync = "GMDTR12";
    public const string UseHandlerSuffix = "GMDTR13";
}