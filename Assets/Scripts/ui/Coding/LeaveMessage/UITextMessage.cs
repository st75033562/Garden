using UnityEngine;
using UnityEngine.UI;

public class UITextMessage : MessageElementBase
{
    public Text m_MessageText;
    public RectTransform m_MessageRectTrans;

    public override LeaveMessage Message
    {
        get { return base.Message; }
        set
        {
            base.Message = value;

            m_MessageText.text = value.m_TextMsg;
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_MessageRectTrans);
            var bottom = m_MessageRectTrans.rect.yMin + m_MessageRectTrans.localPosition.y;
            var rectTrans = GetComponent<RectTransform>();
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -bottom);
        }
    }
}
