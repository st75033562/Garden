using UnityEngine;
using System.Collections;

public class LinkSlotSubDownMenuPlugins : NodePluginsBase
{
	public LinkSlotPlugins m_Linker;

    private UIMenuConfig m_config;

    public override void DecodeClickedCMD(string cmd)
    {
        m_config = UIMenuConfig.Parse(cmd);
    }

    public override void Clicked()
    {
        if (m_config.items.Length > 0)
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UIMenuDialog>();
            m_config.target = RectTransform;
            dialog.Configure(m_config, InputCallBack, CodeContext.panelZoomFactor);
            OpenDialog(dialog);
        }
    }

    public override void InputCallBack(string str)
    {
        m_Linker.InputCallBack(str);

        EventBus.Default.AddEvent(EventId.GuideInput, new GuideInputBackData(gameObject, str));
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_config = ((LinkSlotSubDownMenuPlugins)other).m_config;
    }
}
