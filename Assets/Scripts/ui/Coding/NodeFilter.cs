using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NodeFilter : MonoBehaviour
{
    public event Action<NodeCategory> onCategorySelected;

	public GameObject[] m_Buttons;
    public NodeCategoryTitleConfig m_TitleConfig;

    private NodeFilterData m_filterData;

    void Awake()
    {
        Level = BlockLevel.Advanced;
        InputEnabled = true;
    }

	public void Refresh()
	{
		for (int i = 0; i < m_Buttons.Length; ++i)
		{
			m_Buttons[i].SetActive(false);
		}

        foreach (var filter in ActiveCategories)
        {
			m_Buttons[(int)filter].SetActive(true);
        }
	}

    public NodeFilterData NodeFilterData
    {
        get { return m_filterData; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_filterData = value;
        }
    }

    public IList<NodeCategory> ActiveCategories
    {
        get
        {
            return NodeFilterData.GetCategories((int)Level);
        }
    }

    public bool HasCategory(NodeCategory category)
    {
        return ActiveCategories.Contains(category);
    }

	public void SelectType(int type)
	{
        if (!InputEnabled) { return; }

        if (onCategorySelected != null)
        {
            onCategorySelected((NodeCategory)type);
        }
	}

    public BlockLevel Level
    {
        get;
        set;
    }

    public bool InputEnabled
    {
        get;
        set;
    }
}
