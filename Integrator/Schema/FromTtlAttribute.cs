namespace Biz.Morsink.Integrator.Schema;

[AttributeUsage(AttributeTargets.Class)]
public class FromTtlAttribute : Attribute
{
    public string File { get; }
    public string PreferredPrefix { get; }
    public Uri PrefixUri { get; }

    public FromTtlAttribute(string file, string preferredPrefix, string prefixUri)
    {
        File = file;
        PreferredPrefix = preferredPrefix;
        PrefixUri = new (prefixUri);
    }
}
