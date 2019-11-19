using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BundleViewer : EditorWindow
{
    private Vector2 m_bundlesViewScrollPos;
    private Vector2 m_assetsViewScrollPos;

    private BundleViewerStyles m_styles;
    private BundleInfo m_selectedBundle;
    private BundleInfo m_nextSelectedBundle;

    private BundleAssetInfo m_selectedAsset;
    private BundleAssetInfo m_nextSelectedAsset;

    private BundleDatabase m_database = new BundleDatabase();
    private readonly List<BundleInfo> m_highlightedDuplicateBundles = new List<BundleInfo>();

    private const string StylesPath = "Assets/Editor/BundleViewer/Styles/styles.guiskin";

    void OnEnable()
    {
        m_styles = new BundleViewerStyles(AssetDatabase.LoadAssetAtPath<GUISkin>(StylesPath));
        Refresh();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            Refresh();
        }
        if (GUILayout.Button("Build", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        m_bundlesViewScrollPos = GUILayout.BeginScrollView(m_bundlesViewScrollPos, m_styles.bundlesView);
        foreach (var bundle in m_database.bundles)
        {
            if (DrawBundleItem(bundle, m_selectedBundle == bundle) && bundle != m_selectedBundle)
            {
                m_highlightedDuplicateBundles.Clear();
                m_nextSelectedBundle = bundle;
            }
        }
        GUILayout.EndScrollView();

        m_assetsViewScrollPos = EditorGUILayout.BeginScrollView(m_assetsViewScrollPos);
        if (m_selectedBundle != null)
        {
            ShowBundleContents(m_selectedBundle);
        }
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }

    void Update()
    {
        if (m_nextSelectedBundle != null)
        {
            m_selectedBundle = m_nextSelectedBundle;
            m_selectedAsset = null;
            m_nextSelectedBundle = null;
        }

        if (m_nextSelectedAsset != null)
        {
            m_selectedAsset = m_nextSelectedAsset;
            m_nextSelectedAsset = null;
        }
    }

    bool DrawBundleItem(BundleInfo bundle, bool selected)
    {
        GUIStyle style = m_styles.GetBundleItem(selected);
        if (!selected && m_highlightedDuplicateBundles.Contains(bundle))
        {
            style = m_styles.duplicateBundle;
        }
        GUILayout.BeginHorizontal(style);
        GUILayout.Label(bundle.name, m_styles.GetBundleItem(selected));
        switch (Event.current.type)
        {
        case EventType.MouseDown:
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                selected = true;
            }
            Repaint();
            break;
        }

        if (bundle.hasDuplicateAssets)
        {
            GUILayout.Label("", m_styles.duplicateAsset);
        }
        GUILayout.EndHorizontal();
        return selected;
    }

    void ShowBundleContents(BundleInfo bundle)
    {
        EditorGUILayout.BeginVertical();
        ShowBundleAssets("Includes", bundle.assets);
        ShowBundleAssets("Dependencies", bundle.autoIncludedAssets);
        EditorGUILayout.EndVertical();
    }

    void ShowBundleAssets(string header, List<BundleAssetInfo> assets)
    {
        EditorGUILayout.LabelField(header, m_styles.dependencyHeader);
        foreach (var asset in assets)
        {
            var selected = DrawBundleAsset(asset, m_selectedAsset == asset);
            if (selected && m_selectedAsset != asset)
            {
                m_selectedAsset = asset;
                m_highlightedDuplicateBundles.Clear();
                if (m_selectedAsset.duplicate)
                {
                    m_highlightedDuplicateBundles.AddRange(m_selectedAsset.implicitBundles);
                }
                var obj = AssetDatabase.LoadAssetAtPath(asset.path, typeof(UnityEngine.Object));
                if (obj)
                {
                    EditorGUIUtility.PingObject(obj);
                }
                else
                {
                    Debug.Log("please refresh");
                }
            }
        }
    }

    bool DrawBundleAsset(BundleAssetInfo asset, bool selected)
    {
        GUILayout.BeginHorizontal(m_styles.GetBundleItem(selected));
        var objIcon = AssetDatabase.GetCachedIcon(asset.path);
        GUILayout.Label(new GUIContent(objIcon), m_styles.assetIcon);
        GUILayout.Label(asset.name, m_styles.GetBundleItem(selected));
        if (asset.duplicate)
        {
            GUILayout.Label("", m_styles.duplicateAsset);
        }
        GUILayout.EndHorizontal();

        switch (Event.current.type)
        {
        case EventType.MouseDown:
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                selected = true;
            }
            Repaint();
            break;
        }
        return selected;
    }

    void Refresh()
    {
        m_database.Build();
    }

    [MenuItem("Window/Bundle Viewer")]
    static void Open()
    {
        EditorWindow.GetWindow<BundleViewer>();
    }
}