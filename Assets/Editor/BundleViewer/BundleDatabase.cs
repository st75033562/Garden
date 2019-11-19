using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class BundleInfo
{
    public string name;

    public readonly List<BundleAssetInfo> assets = new List<BundleAssetInfo>();

    public readonly List<BundleAssetInfo> autoIncludedAssets = new List<BundleAssetInfo>();

    public readonly HashSet<BundleInfo> dependentBundles = new HashSet<BundleInfo>();

    public bool hasDuplicateAssets
    {
        get
        {
            return autoIncludedAssets.Any(x => x.duplicate);
        }
    }
}

public class BundleAssetInfo
{
    public string path;

    public string name { get { return Path.GetFileNameWithoutExtension(path); } }

    public BundleInfo assignedBundle;

    public readonly HashSet<BundleAssetInfo> dependentAssets = new HashSet<BundleAssetInfo>();

    // non empty only if assigedBundle is null
    public readonly HashSet<BundleInfo> implicitBundles = new HashSet<BundleInfo>();

    public int referenceCount;

    public bool duplicate
    {
        get
        {
            if (assignedBundle != null)
            {
                return false;
            }
            return implicitBundles.Count > 1;
        }
    }
}

public class BundleDatabase
{
    private readonly Dictionary<string, BundleInfo> m_bundles = new Dictionary<string, BundleInfo>();
    private readonly Dictionary<string, BundleAssetInfo> m_assets = new Dictionary<string, BundleAssetInfo>();

    public IEnumerable<BundleInfo> bundles
    {
        get { return m_bundles.Values; }
    }

    public IEnumerable<BundleAssetInfo> assets
    {
        get { return m_assets.Values; }
    }

    public void Build()
    {
        m_bundles.Clear();
        m_assets.Clear();

        var autoIncludedAssets = new HashSet<string>();

        foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            autoIncludedAssets.Clear();
            var bundle = GetBundle(bundleName);

            foreach (var assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
            {
                var asset = GetAsset(assetPath);
                ++asset.referenceCount;
                asset.assignedBundle = bundle;
                bundle.assets.Add(asset);

                CollectDependencies(autoIncludedAssets, asset, bundle);
            }
        }
    }

    // collect dependencies of the asset in the given bundle
    private void CollectDependencies(HashSet<string> autoIncludedAssets, BundleAssetInfo asset, BundleInfo bundle)
    {
        foreach (var dep in AssetDatabase.GetDependencies(asset.path, false))
        {
            // ignore script
            var type = AssetDatabase.GetMainAssetTypeAtPath(dep);
            if (type == typeof(MonoScript))
            {
                continue;
            }

            var dependency = GetAsset(dep);
            ++dependency.referenceCount;
            dependency.dependentAssets.Add(asset);

            var importer = AssetImporter.GetAtPath(dep);
            if (importer.assetBundleName != "")
            {
                if (importer.assetBundleName != bundle.name)
                {
                    var dependencyBundle = GetBundle(importer.assetBundleName);
                    dependencyBundle.dependentBundles.Add(bundle);
                    CollectDependencies(null, dependency, dependencyBundle);
                }
                continue;
            }

            if (autoIncludedAssets != null)
            {
                if (autoIncludedAssets.Contains(dep))
                {
                    continue;
                }

                bundle.autoIncludedAssets.Add(dependency);
                dependency.implicitBundles.Add(bundle);

                autoIncludedAssets.Add(dep);
            }

            CollectDependencies(autoIncludedAssets, dependency, bundle);
        }
    }

    private BundleInfo GetBundle(string name)
    {
        BundleInfo bundle;
        if (!m_bundles.TryGetValue(name, out bundle))
        {
            bundle = new BundleInfo();
            bundle.name = name;
            m_bundles.Add(name, bundle);
        }
        return bundle;
    }

    private BundleAssetInfo GetAsset(string path)
    {
        BundleAssetInfo asset;
        if (!m_assets.TryGetValue(path, out asset))
        {
            asset = new BundleAssetInfo();
            asset.path = path;
            m_assets.Add(path, asset);
        }
        return asset;
    }
}
