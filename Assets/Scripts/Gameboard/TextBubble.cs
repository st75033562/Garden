using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    /// <summary>
    /// if min width and max width are both 0, then fixed width
    /// if min height and max height are both 0, then fixed height
    /// </summary>
    public class TextBubble : TextObject
    {
        public Text m_text;
        public int m_minWidth;
        public int m_maxWidth;
        public int m_minHeight;
        public int m_maxHeight;

        private RectTransform m_textParent;
        private Vector4 m_padding;

        void Awake()
        {
            m_textParent = m_text.transform.parent.GetComponent<RectTransform>();

            if (m_maxWidth > 0 && m_minWidth <= 0)
            {
                Debug.LogError("maxWidth and minWidth must both be positive or 0");
                m_maxWidth = 0;
            }

            if (m_maxHeight > 0 && m_minHeight <= 0)
            {
                Debug.LogError("maxHeight and minHeight must both be positive or 0");
                m_maxHeight = 0;
            }
        }

        public override void SetText(string str, int size, Color color)
        {
            m_text.text = str;
            m_text.fontSize = size;
            m_text.color = color;

            // update width first, preferred height is dependent on the width of the Text
            float width = m_text.preferredWidth + m_text.rectTransform.offsetMin.x - m_text.rectTransform.offsetMax.x;
            if (m_minWidth > 0 && m_maxWidth > 0) 
            {
                width = Mathf.Clamp(width, m_minWidth, m_maxWidth);
            }
            else if (m_minWidth > 0)
            {
                width = Mathf.Max(m_minWidth, width);
            }

            m_textParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            float height = m_text.preferredHeight + m_text.rectTransform.offsetMin.y - m_text.rectTransform.offsetMax.y;
            if (m_minHeight > 0 && m_maxHeight > 0)
            {
                height = Mathf.Clamp(height, m_minHeight, m_maxHeight);
            }
            else if (m_minHeight > 0)
            {
                height = Mathf.Max(m_minHeight, height);
            }

            m_textParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
