using UnityEngine;

public class NodeCategoryTitleConfig : ScriptableObject
{
    public Sprite[] m_Icons;
    public string[] m_LocIds;

    public Sprite GetIcon(NodeCategory cate)
    {
        return m_Icons[(int)cate];
    }

    public string GetLocId(NodeCategory cate)
    {
        return m_LocIds[(int)cate];
    }
}
