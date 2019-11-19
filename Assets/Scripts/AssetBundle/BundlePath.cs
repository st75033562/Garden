using System.Linq;

public static class BundlePath
{
    public const char PathSeparator = '-';

    public static string GetName(params string[] dirs)
    {
        return string.Join(PathSeparator.ToString(), dirs.Select(x => x.ToLowerInvariant()).ToArray());
    }
}
