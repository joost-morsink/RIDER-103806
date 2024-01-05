using System;
using Biz.Morsink.Integrator.Schema;

namespace Biz.Morsink.Integrator.SourceSystems;

[FromTtl("test.ttl", "tst", "http://integrator.morsink.biz/test/")]
public static partial class CodegenTest
{
    public static void Main()
    {
        Console.WriteLine(TheAnswer);
    }
}   