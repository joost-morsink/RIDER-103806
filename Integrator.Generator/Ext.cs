using Microsoft.CodeAnalysis;

namespace Biz.Morsink.Integrator.Generator;

public static class Ext
{
    public static Diagnostic CreateFor(this DiagnosticDescriptor descriptor, GeneratorSyntaxContext context, params object[] args)
        => Diagnostic.Create(descriptor, context.Node.GetLocation(), args);

    public static Diagnostic CreateAt(this DiagnosticDescriptor descriptor, Location? location, params object[] args)
        => Diagnostic.Create(descriptor, location, args);
}