using UnityEngine;
using System.Collections;
using System;

public class LinkSlotSubSlotPlugins : NumInputPlugins
{
	public LinkSlotPlugins m_Linker;

	const float m_TextOffset = 48;

    public override void InputCallBack(string str)
    {
        m_Linker.InputCallBack(str);
    }

	public override void SetPluginsText(string str)
	{
		m_TextKey = str;
		if (m_Text)
		{
			m_Text.text = str.Localize();
			float mWidth = (float)Math.Ceiling(m_Text.preferredWidth);
			mWidth += m_TextOffset;
            if (NodeTemplateCache.Instance.ShowBlockUI)
            {
                m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mWidth);
            }

        }
	}
}
