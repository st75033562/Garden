using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageMenuPlugins : DownMenuPlugins
{
    private static readonly Color GlobalMessageColor = Color.red;

    protected override void Awake()
    {
        base.Awake();

        InitListeners();
    }

    private void InitListeners()
    {
        if (CodeContext != null)
        {
            CodeContext.messageManager.onMessageDeleted += OnMessageDeleted;
        }
    }

    protected void OnDestroy()
    {
        if (CodeContext != null)
        {
            CodeContext.messageManager.onMessageDeleted -= OnMessageDeleted;
        }
    }

	public override void Clicked()
	{
        var items = GetMenuItems(CodeContext.messageManager.globalMessages, GlobalMessageColor)
                            .Concat(GetMenuItems(CodeContext.messageManager.localMessages));
        SetMenuItems(items);
		base.Clicked();
	}

    public override void ResetSelection()
    {
        SetPluginsText(Message.defaultMessage.name);
        LayoutChanged();
    }

    private void OnMessageDeleted(Message msg)
    {
        if (msg.name == m_TextKey)
        {
            ResetSelection();
        }
    }

    private IEnumerable<UIMenuItem> GetMenuItems(IEnumerable<Message> messages, Color? color = null)
    {
        if (color == null)
        {
            color = UIMenuItem.DefaultColor;
        }
        return messages.OrderBy(x => x.name).Select(x => new UIMenuItem(x.name, color.Value));
    }

    public override void SetPluginsText(string str)
    {
        base.SetPluginsText(str);
        if (CodeContext != null)
        {
            var msg = CodeContext.messageManager.get(str);
            m_Text.color = msg != null && msg.scope == NameScope.Global ? GlobalMessageColor : UIMenuItem.DefaultColor;
        }
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        InitListeners();
    }
}
