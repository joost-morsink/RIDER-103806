using System;

namespace Biz.Morsink.Integrator.Generator;

public class FromTtl : IEquatable<FromTtl>
{
    public string File { get; }
    public string Ns { get; }
    public string Class { get; }
    public string PreferredPrefix { get; }
    public Uri PrefixUri { get; }

    public FromTtl(string file, string @namespace, string @class, string preferredPrefix, string prefixUri)
    {
        File = file;
        Ns = @namespace;
        Class = @class;
        PreferredPrefix = preferredPrefix;
        PrefixUri = new(prefixUri);
    }

    public bool Equals(FromTtl? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && File == other.File && Ns == other.Ns && Class == other.Class &&
               PreferredPrefix == other.PreferredPrefix && PrefixUri.Equals( other.PrefixUri);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FromTtl) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(File, Ns, Class, PreferredPrefix, PrefixUri);
    }

    public static bool operator ==(FromTtl? left, FromTtl? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(FromTtl? left, FromTtl? right)
    {
        return !Equals(left, right);
    }
}