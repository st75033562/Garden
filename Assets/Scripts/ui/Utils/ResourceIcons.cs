using System;
using UnityEngine;

public class ResourceIcons : ScriptableObject
{
    [SerializeField]
    private Sprite[] m_icons;

    public Sprite GetIcon(ResType type)
    {
        return m_icons[(int)type - 1];
    }
}
