using VDS.RDF;

namespace Biz.Morsink.Integrator.Schema;

public class Generator
{
    private readonly string _namespace;
    private readonly string _class;

    public Generator(string @namespace, string @class)
    {
        _namespace = @namespace;
        _class = @class;
    }

    public Task<string> Generate()
    {
        return Task.FromResult(new InternalGenerator(this).Generate());
    }


    private record InternalGenerator(Generator parent)
    {
        public string Generate()
        {
            return $@"using System;
using System.Collections.Immutable;
using Biz.Morsink.Integrator.Schema;
using VDS.RDF;

#nullable enable

namespace {parent._namespace};

public static partial class {parent._class}
{{
    public static int TheAnswer => 42;
}} 
";
        }
    }
}