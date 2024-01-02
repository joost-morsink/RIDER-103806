using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Biz.Morsink.Integrator.Generator;

public class Result<T>
{
    private readonly T _value;
    private ImmutableList<Diagnostic> _diagnostics;
    public Result(T value)
    {
        _value = value;
        _diagnostics = ImmutableList<Diagnostic>.Empty;
    }
    public Result(IEnumerable<Diagnostic> diagnostics)
    {
        _value = default!;
        _diagnostics = diagnostics.ToImmutableList();
    }
    
    public bool HasValue => _diagnostics.IsEmpty;
    public T Value => HasValue ? _value : throw new InvalidOperationException("Result has no value.");
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    
    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Diagnostic diagnostic) => new(ImmutableList.Create(diagnostic));
}