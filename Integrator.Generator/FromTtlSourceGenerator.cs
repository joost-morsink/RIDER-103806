using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Biz.Morsink.Integrator.Generator;

[Generator]
public class FromTtlSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var ttlClasses = context.SyntaxProvider.CreateSyntaxProvider(
                (syntax, _) => syntax is ClassDeclarationSyntax {AttributeLists.Count: > 0},
                TryCreate)
            .Where(x => x is not null)
            .Select((x, _) => x!);
        var ttlFiles =
            context.AdditionalTextsProvider.Where(x => x.Path.EndsWith(".ttl", StringComparison.OrdinalIgnoreCase))
                .Select(LoadTtl)
                .Where(x => x.HasValue)
                .Collect();
        var turtles = ttlFiles
                .Select((graphs, _) =>
                    graphs.ToImmutableDictionary(g => g!.Value.Item1, g => g!.Value.Item2));

        var combination = ttlClasses.Combine(turtles);

        context.RegisterSourceOutput(combination, (spc, cls) =>
        {
            if (!cls.Left.HasValue)
            {
                foreach (var diag in cls.Left.Diagnostics)
                    spc.ReportDiagnostic(diag);
                return;
            }

            var source = GetSource(cls.Left.Value, cls.Right);
            if (!source.HasValue)
            {
                foreach (var diag in source.Diagnostics)
                    spc.ReportDiagnostic(diag);
                return;
            }

            spc.AddSource($"{cls.Left.Value.Class}.g.cs", source.Value);
        });
    }

    private Result<string> GetSource(FromTtl attribute, ImmutableDictionary<string, Graph> graphs)
    {
        var builder = new StringBuilder();
        var tripleStore = new TripleStore();
        if (!graphs.TryGetValue(attribute.File, out var graph))
            return CannotFindLinkedTtlFile.CreateAt(null, attribute.File);
        tripleStore.Graphs.Add(graph, true);

        var generator = new Schema.Generator(attribute.Ns, attribute.Class);
        return generator.Generate().Result;
    }

    public (string, Graph)? LoadTtl(AdditionalText additionalText, CancellationToken cancel)
    {
        var text = additionalText.GetText(cancel);
        if (text is null)
            return default;
        var parser = new TurtleParser();
        var graph = new Graph();
        parser.Load(graph, new StringReader(text.ToString()));
        return (Path.GetFileName(additionalText.Path), graph);
    }
    
    public Result<FromTtl>? TryCreate(GeneratorSyntaxContext generatorContext, CancellationToken cancel)
    {
        if (generatorContext.Node is not ClassDeclarationSyntax generatorContextNode)
            return NotAClassDeclaration.CreateFor(generatorContext);

        foreach (var attributeList in generatorContextNode.AttributeLists)
        {
            cancel.ThrowIfCancellationRequested();

            foreach (var attribute in attributeList.Attributes)
            {
                if (generatorContext.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == "Biz.Morsink.Integrator.Schema.FromTtlAttribute")
                {
                    var literals = attribute.ArgumentList!.Arguments.Select(a => a.Expression)
                        .OfType<LiteralExpressionSyntax>().ToArray();
                    if (literals.Length < 3)
                        return IncorrectNumberOfArgumentsToTtl.CreateFor(generatorContext, literals.Length);
                    var symbol = generatorContext.SemanticModel.GetDeclaredSymbol(generatorContextNode);
                    if (symbol is null)
                        return CannotResolveDeclaredSymbol.CreateFor(generatorContext, generatorContextNode.Identifier.ValueText);
                    var ns = symbol.ContainingNamespace.ToDisplayString();
                    var tn = symbol.Name;
                    return new FromTtl(GetValue(literals[0]), ns, tn, GetValue(literals[1]), GetValue(literals[2]));
                }
            }
        }

        return null;

        string GetValue(LiteralExpressionSyntax expr)
        {
            var res = generatorContext.SemanticModel.GetConstantValue(expr);
            return res.HasValue ? res.Value!.ToString()! : throw new ArgumentException(nameof(expr));
        }
    }

    private static DiagnosticDescriptor NotAClassDeclaration { get; } =
        new("ISS0001", "Not a class declaration.", "Not a class declaration.", "", DiagnosticSeverity.Error, true);
    private static DiagnosticDescriptor IncorrectNumberOfArgumentsToTtl { get; } =
        new("ISS0002", "Incorrect number of arguments to FromTtl attribute.", "Incorrect number of arguments to FromTtl attribute. ({0}!=3)", "", DiagnosticSeverity.Error, true);
    private static DiagnosticDescriptor CannotResolveDeclaredSymbol { get; } =
        new("ISS0003", "Cannot resolve declared symbol.", "Cannot resolve declared symbol {0}.", "", DiagnosticSeverity.Error, true);

    private static DiagnosticDescriptor CannotFindLinkedTtlFile { get; } =
        new ("ISS0004", "Cannot find linked ttl file.", "Cannot find linked ttl file {0}.", "", DiagnosticSeverity.Error, true);
}