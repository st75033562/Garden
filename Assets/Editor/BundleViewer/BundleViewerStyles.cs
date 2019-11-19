using UnityEngine;

public class BundleViewerStyles
{
    private GUISkin m_skin;

    public BundleViewerStyles(GUISkin skin)
    {
        m_skin = skin;
    }

    public GUIStyle GetBundleItem(bool selected)
    {
        return GetStyle("bundleItem", selected);
    }

    public GUIStyle assetIcon
    {
        get { return GetStyle("assetIcon", false); }
    }

    public GUIStyle dependencyHeader
    {
        get { return GetStyle("dependencyHeader", false); }
    }

    public GUIStyle duplicateAsset
    {
        get { return GetStyle("duplicateAsset", false); }
    }

    public GUIStyle bundlesView
    {
        get { return GetStyle("bundlesView", false); }
    }

    public GUIStyle duplicateBundle
    {
        get { return GetStyle("duplicateBundle", false); }
    }

    private GUIStyle GetStyle(string prefix, bool selected)
    {
        return m_skin.GetStyle(prefix + (selected ? "Selected" : ""));
    }
}
