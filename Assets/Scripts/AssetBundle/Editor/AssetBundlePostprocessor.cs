using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class AssetBundlePostprocessor : AssetPostprocessor
{
    public const string RootNoSlash = "Assets/AssetBundles";
    public const string Root = RootNoSlash + "/";

    private const string SharedMaterialBundleName = "shared_materials";
    private static readonly char[] InvalidBundlePathChars = new[] { BundlePath.PathSeparator, '.' };
    private const char VariantPrefix = '@';
    private const string LabelObject = "object";

    private class AssetBundleName
    {
        public string name;
        public string variant;

        public AssetBundleName()
        {
        }

        public AssetBundleName(string name, string variant)
        {
            this.name = name;
            this.variant = variant;
        }

        // path is the containing folder of the bundle asset
        public static AssetBundleName Parse(string path)
        {
            var bundleName = new AssetBundleName();
            var lastFolderName = Path.GetFileName(path);
            if (lastFolderName[0] == VariantPrefix)
            {
                bundleName.variant = lastFolderName.Substring(1);
                path = Path.GetDirectoryName(path);
            }
            bundleName.name = GetAssetBundleName(path);

            return bundleName;
        }

        private static string GetAssetBundleName(string path)
        {
            string relativePath = path;
            if (path.StartsWith(Root))
            {
                relativePath = path.Substring(Root.Length);
            }
            return relativePath.Replace('/', BundlePath.PathSeparator);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as AssetBundleName;
            if (rhs == null)
            {
                return false;
            }

            return name == rhs.name && variant == rhs.variant;
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var ruleDB = new AssetBundleRuleDatabase();
        ruleDB.Load(Root + "rules.txt");

        var bundleAssetPaths = importedAssets.Concat(movedAssets).Where(x => x.StartsWith(Root));

        foreach (var path in bundleAssetPaths)
        {
            if (!ruleDB.Accept(path) || Directory.Exists(path))
            {
                SetAssetBundleName(path, null);
                continue;
            }

            var parentFolder = Path.GetDirectoryName(path);
            if (parentFolder.IndexOfAny(InvalidBundlePathChars) != -1)
            {
                Debug.LogErrorFormat("parent folder of asset {0} contains reserved char {1}", path, InvalidBundlePathChars);
                return;
            }

            // ignore assets in the Root folder
            if (parentFolder + "/" == Root)
            {
                continue;
            }

            ConfigureAsset(path);
            if (path.EndsWith(".prefab"))
            {
                ValidatePrefab(path);
            }
        }

        for (int i = 0; i < movedAssets.Length; ++i)
        {
            if (!movedAssets[i].StartsWith(Root) && movedFromAssetPaths[i].StartsWith(Root))
            {
                ClearConfiguration(movedAssets[i]);
            }
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    private static void ConfigureAsset(string path)
    {
        SetAssetBundleName(path, AssetBundleName.Parse(Path.GetDirectoryName(path)));
        if (IsObject(path))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EditorUtils.AddLabel(asset, LabelObject);
        }
    }

    private static void ValidatePrefab(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go)
        {
            if (go.transform.eulerAngles != Vector3.zero)
            {
                Debug.LogWarning("root rotation should be zero, reset to zero", go);
                go.transform.eulerAngles = Vector3.zero;
            }

            if (go.transform.localScale != Vector3.one)
            {
                Debug.LogWarning("root scale should be identity, reset to identity", go);
                go.transform.localScale = Vector3.one;
            }
        }
    }

    private static void ClearConfiguration(string path)
    {
        SetAssetBundleName(path, null);
        if (IsObject(path))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EditorUtils.RemoveLabel(asset, LabelObject);
        }
    }

    private static bool IsObject(string path)
    {
        return path.IndexOf("/obj/") != -1;
    }

    private static void SetAssetBundleName(string path, AssetBundleName name)
    {
        if (name != null)
        {
            SetAssetBundleName(path, name.name, name.variant);
        }
        else
        {
            SetAssetBundleName(path, null, null);
        }
    }

    private static void SetAssetBundleName(string path, string name, string variant = null)
    {
        var importer = AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.assetBundleName = name;
            if (name != null)
            {
                importer.assetBundleVariant = variant;
            }
        }
    }

    [MenuItem("Tools/Asset Bundle/Resolve Duplicate Assets")]
    private static void ResolveDuplicateAssets()
    {
        var database = new BundleDatabase();
        database.Build();

        var excludedBundles = File.ReadAllLines(Root + "excluded_bundles.txt");
        var sharedMaterials = new List<string>();

        var ruleDB = new AssetBundleRuleDatabase();
        ruleDB.Load(Root + "rules.txt");

        foreach (var asset in database.assets)
        {
            if (!ruleDB.Accept(asset.path))
            {
                SetAssetBundleName(asset.path, null);
                continue;
            }

            if (asset.duplicate)
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(asset.path);
                if (assetType == typeof(Material))
                {
                    var bundleName = GuessMaterialBundleName(asset.path);
                    if (bundleName != null)
                    {
                        SetAssetBundleName(asset.path, bundleName);
                    }
                    else
                    {
                        sharedMaterials.Add(asset.path);
                    }
                }
                else if (assetType == typeof(Shader))
                {
                    sharedMaterials.Add(asset.path);
                }
                else
                {
                    var parentFolderPath = Path.GetDirectoryName(asset.path);
                    SetAssetBundleName(asset.path, AssetBundleName.Parse(parentFolderPath));
                }
            }
            else if (asset.assignedBundle != null && asset.referenceCount < 2 && !IsUserCreatedBundle(asset.assignedBundle.name))
            {
                if (excludedBundles.Contains(asset.assignedBundle.name))
                {
                    continue;
                }

                SetAssetBundleName(asset.path, null);
            }
        }

        if (sharedMaterials.Count > 0)
        {
            foreach (var mat in sharedMaterials)
            {
                SetAssetBundleName(mat, SharedMaterialBundleName);
            }
        }
    }

    private static int GetDependentUserCreatedBundleCount(BundleInfo bundle)
    {
        var userBundles = new HashSet<BundleInfo>();
        foreach (var dep in bundle.dependentBundles)
        {
            GetDependentUserCreatedBundles(dep, userBundles);
        }
        return userBundles.Count;
    }

    private static void GetDependentUserCreatedBundles(BundleInfo bundle, HashSet<BundleInfo> userBundles)
    {
        if (IsUserCreatedBundle(bundle.name))
        {
            userBundles.Add(bundle);
        }

        foreach (var dep in bundle.dependentBundles)
        {
            GetDependentUserCreatedBundles(dep, userBundles);
        }
    }

    private static bool IsUserCreatedBundle(string bundleName)
    {
        string variant = null;
        int dotIndex = bundleName.LastIndexOf('.');
        if (dotIndex != -1)
        {
            variant = bundleName.Substring(dotIndex + 1);
            bundleName = bundleName.Substring(0, dotIndex);
        }
        string relativePath = string.Join("/", bundleName.Split(BundlePath.PathSeparator));
        if (variant != null)
        {
            bundleName += "/@" + variant;
        }
        return Directory.Exists(Root + relativePath);
    }

    private static AssetBundleName GuessMaterialBundleName(string path)
    {
        AssetBundleName bundleName = null;
        foreach (var dep in AssetDatabase.GetDependencies(path, false))
        {
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(dep);
            if (assetType.IsSubclassOf(typeof(Texture)))
            {
                var curBundleName = AssetBundleName.Parse(Path.GetDirectoryName(dep));
                if (bundleName == null)
                {
                    bundleName = curBundleName;
                }
                else if (!bundleName.Equals(curBundleName))
                {
                    bundleName = null;
                    break;
                }
            }
        }
        return bundleName;
    }
}