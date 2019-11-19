using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Linq;

public class UIVarItem : MonoBehaviour, ILayoutElement
{
    public Text nameText;
    public Text valueText;

    private BaseVariable m_data;
    private RectTransform m_rectTransform;

    public RectTransform rectTransform
    {
        get
        {
            if (!m_rectTransform)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }
            return m_rectTransform;
        }
    }


    public void Init(BaseVariable data)
    {
        Assert.IsNotNull(data);

        Unsubscribe();
        m_data = data;
        nameText.text = data.name;
        data.onChanged += OnValueChanged;
        OnValueChanged(m_data);
    }

    private void Unsubscribe()
    {
        if (m_data != null)
        {
            m_data.onChanged -= OnValueChanged;
        }
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValueChanged(BaseVariable data)
    {
        switch (data.type)
        {
        case BlockVarType.Variable:
        	valueText.text = ((VariableData)data).getString();
            break;

        case BlockVarType.List:
        case BlockVarType.Stack:
        case BlockVarType.Queue:
            valueText.text = string.Join(",", ((BaseVarCollection)data).ToArray());
            break;
        }

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    #region ILayoutElement
    public void CalculateLayoutInputHorizontal()
    {
    }

    public void CalculateLayoutInputVertical()
    {
    }

    public float flexibleHeight
    {
        get { return -1; }
    }

    public float flexibleWidth
    {
        get { return -1; }
    }

    public int layoutPriority
    {
        get { return -1; }
    }

    public float minHeight
    {
        get { return -1; }
    }

    public float minWidth
    {
        get { return -1; }
    }

    public float preferredHeight
    {
        get
        {
            return Mathf.Max(nameText.preferredHeight, valueText.preferredHeight);
        }
    }

    public float preferredWidth
    {
        get { return rectTransform.rect.width; }
    }

    #endregion
}
