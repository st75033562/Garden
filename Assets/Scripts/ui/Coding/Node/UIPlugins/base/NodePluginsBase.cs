using System;
using UnityEngine;
using UnityEngine.UI;

public class NodePluginChangedEvent
{
    public NodePluginChangedEvent(NodePluginsBase plugin, Save_PluginsData oldState, Save_PluginsData newState)
    {
        this.plugin = plugin;
        this.oldState = oldState;
        this.newState = newState;
    }

    public NodePluginsBase plugin { get; private set; }

    public Save_PluginsData oldState { get; private set; }

    public Save_PluginsData newState { get; private set; }
}

public class NodePluginsBase : MonoBehaviour, IDialogInputCallback, IDialogInputValidator
{
    public static event Action<NodePluginsBase, UIDialog> onBeginInputEdit;
    public static event Action<NodePluginsBase, UIDialog> onEndInputEdit;

    [SerializeField]
    private Vector2 m_PosOffset;

    [SerializeField]
    private Vector2 m_SizeOffset;

    [ReadOnly]
    [SerializeField]
    protected string m_TextKey;

    public Text m_Text;
    protected RectTransform m_Rect;

    [SerializeField] // for cloning
    [ReadOnly]
    protected FunctionNode m_MyNode;

    protected Vector2 m_OriginalSize;

    protected virtual void Awake()
    {
        m_Rect = GetComponent<RectTransform>();
        m_OriginalSize = m_Rect.rect.size;
    }

    protected virtual void Start()
    {
    }

    public RectTransform RectTransform
    {
        get { return m_Rect; }
    }

    public Vector2 PosOffset
    {
        get { return m_PosOffset; }
        set { m_PosOffset = value; }
    }

    public Vector2 SizeOffset
    {
        get { return m_SizeOffset; }
        set { m_SizeOffset = value; }
    }

    public virtual void Clicked()
    {
        //print(name + " plugins has been clicked");
    }

    protected void OpenDialog(UIInputDialogBase dialog)
    {
        Action onClosed = null;
        onClosed = () => {
            if (onEndInputEdit != null)
            {
                onEndInputEdit(this, dialog.dialogType);
            }

            dialog.onClosed -= onClosed;
        };

        dialog.onClosed += onClosed;
        dialog.OpenDialog();

        if (onBeginInputEdit != null)
        {
            onBeginInputEdit(this, dialog.dialogType);
        }
    }

    public virtual void InputCallBack(string str)
    {
        var oldState = GetPluginSaveData();

        OnInput(str);

        var newState = GetPluginSaveData();
        if (!oldState.Equals(newState))
        {
            var args = new NodePluginChangedEvent(this, oldState, newState);
            CodeContext.eventBus.AddEvent(EventId.NodePluginChanged, args);
        }
    }

    protected virtual void OnInput(string str) { }

    protected void MarkChanged()
    {
        LayoutChanged();
    }

    public virtual string ValidateInput(string val)
    {
        return null;
    }

    public virtual void DecodeClickedCMD(string cmd)
    {
    }

    public virtual void CopyDataToTarget(NodePluginsBase target)
    {
        target.m_PosOffset = m_PosOffset;
        target.m_SizeOffset = m_SizeOffset;
        target.m_TextKey = m_TextKey;
        target.PluginID = PluginID;
        target.m_Text = m_Text;
    }

    public virtual Save_PluginsData GetPluginSaveData()
    {
        Save_PluginsData tSaveData = new Save_PluginsData();
        tSaveData.PluginId = PluginID;
        //tSaveData.PluginTextValue = m_TextKey;
        return tSaveData;
    }

    public virtual void LoadPluginSaveData(Save_PluginsData save)
    {
        //SetPluginsText(save.PluginTextValue);
    }

    public virtual void SetPluginsText(string str)
    {
        m_TextKey = str;
        if (m_Text)
        {
            var text = m_TextKey.Localize();
            int index = text.IndexOf('\n');
            string firstLine = index != -1 ? text.Substring(0, index) : text;
            m_Text.text = firstLine;
        }
    }

    protected void ChangePluginsText(string str)
    {
        bool changed = m_TextKey != str;
        SetPluginsText(str);
        if (changed)
        {
            MarkChanged();
        }
    }

    public virtual string GetPluginsText()
    {
        return m_TextKey;
    }

    public virtual FunctionNode ParentNode
    {
        get { return m_MyNode; }
        set { m_MyNode = value; }
    }

    public virtual void Layout()
    {
        if (m_Text)
        {
            var genSettings = m_Text.GetGenerationSettings(Vector2.zero);
            var width = m_Text.cachedTextGenerator.GetPreferredWidth(m_Text.text, genSettings);
            width /= m_Text.pixelsPerUnit;
            // keep the padding
            if (m_Text.rectTransform != RectTransform)
            {
                width -= m_Text.rectTransform.sizeDelta.x;
            }

            m_Rect.SetSize(new Vector2(width, m_OriginalSize.y) + m_SizeOffset);
        }
        else
        {
            m_Rect.SetSize(m_OriginalSize);
        }
    }

    public void LayoutChanged()
    {
        if (m_MyNode)
        {
            m_MyNode.LayoutBottomUp();
        }
    }

    /// <summary>
    /// called after the plugin is positioned, layout any child nodes that depend on the plugin's position
    /// </summary>
    public virtual void LayoutChild()
    {
    }

    public int PluginID
    {
        get;
        set;
    }

    public virtual void PostClone(NodePluginsBase other)
    {
        PluginID = other.PluginID;
    }

    public CodeContext CodeContext
    {
        get { return m_MyNode ? m_MyNode.CodeContext : null; }
    }
}
