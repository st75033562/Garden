using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetBundleTools
{
    [MenuItem("Tools/Asset Bundle/Clean Cache")]
    public static void CleanCache()
    {
        Caching.CleanCache();
    }

    [MenuItem("Tools/Asset Bundle/Dump Assets")]
    public static void DumpAssets()
    {
        var database = new BundleDatabase();
        database.Build();

        using (var fs = new StreamWriter("d:/ab.txt"))
        {
            foreach (var asset in database.assets.OrderBy(x => x.path))
            {
                fs.WriteLine("{0} {1} {2}", asset.path, asset.assignedBundle == null ? "*" : "", asset.referenceCount);
                if (asset.implicitBundles.Count > 0)
                {
                    fs.WriteLine(string.Join("\n", asset.implicitBundles.Select(x => " " + x.name).ToArray()));
                }
                else
                {
                    fs.WriteLine(" " + asset.assignedBundle.name);
                }
            }
        }
    }

    [MenuItem("Tools/Asset Bundle/Dump Asset Dependents")]
    public static void DumpDependentAssets()
    {
        var database = new BundleDatabase();
        database.Build();
        using (var fs = new StreamWriter("d:/deps.txt"))
        {
            foreach (var asset in database.assets.OrderBy(x => x.path))
            {
                DumpDependents(fs, 0, asset);
            }
        }
    }

    static void DumpDependents(StreamWriter fs, int depth, BundleAssetInfo asset)
    {
        fs.Write("".PadLeft(depth * 2));
        fs.WriteLine(asset.path);
        foreach (var dep in asset.dependentAssets)
        {
            DumpDependents(fs, depth + 1, dep);
        }
    }

    [MenuItem("Tools/Asset Bundle/Dump Dependent Bundles")]
    public static void DumpDependentBundles()
    {
        var database = new BundleDatabase();
        database.Build();
        using (var fs = new StreamWriter("d:/dep_abs.txt"))
        {
            foreach (var bundle in database.bundles.OrderBy(x => x.name))
            {
                fs.WriteLine(bundle.name);
                foreach (var dep in bundle.dependentBundles)
                {
                    fs.WriteLine("  " + dep.name);
                }
            }
        }
    }
}
