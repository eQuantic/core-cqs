using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace eQuantic.Core.CQS.Generators;

/// <summary>
/// Analyzer that validates CQS handler implementations at compile-time
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HandlerValidationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ECQS001";
    
    private static readonly LocalizableString Title = "Handler should be sealed or abstract";
    private static readonly LocalizableString MessageFormat = "CQS handler '{0}' should be sealed for better performance, or abstract if it's a base class";
    private static readonly LocalizableString Description = "CQS handlers should be sealed to allow the JIT compiler to devirtualize method calls.";
    private const string Category = "Performance";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public const string MissingHandlerDiagnosticId = "ECQS002";
    
    private static readonly LocalizableString MissingHandlerTitle = "Command/Query has no handler";
    private static readonly LocalizableString MissingHandlerMessageFormat = "No handler found for '{0}'";
    private static readonly LocalizableString MissingHandlerDescription = "Every command and query should have exactly one handler.";

    private static readonly DiagnosticDescriptor MissingHandlerRule = new(
        MissingHandlerDiagnosticId,
        MissingHandlerTitle,
        MissingHandlerMessageFormat,
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MissingHandlerDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule, MissingHandlerRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
            return;

        // Skip if already sealed or abstract
        if (namedTypeSymbol.IsSealed || namedTypeSymbol.IsAbstract)
            return;

        // Check if this class implements any CQS handler interface
        var isHandler = namedTypeSymbol.AllInterfaces.Any(i =>
        {
            var fullName = i.OriginalDefinition.ToDisplayString();
            return fullName.StartsWith("eQuantic.Core.CQS.Handlers.ICommandHandler") ||
                   fullName.StartsWith("eQuantic.Core.CQS.Handlers.IQueryHandler") ||
                   fullName.StartsWith("eQuantic.Core.CQS.Handlers.IPagedQueryHandler") ||
                   fullName.StartsWith("eQuantic.Core.CQS.Notifications.INotificationHandler") ||
                   fullName.StartsWith("eQuantic.Core.CQS.Streaming.IStreamQueryHandler");
        });

        if (!isHandler)
            return;

        // Report diagnostic for non-sealed handler
        var diagnostic = Diagnostic.Create(
            Rule,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
