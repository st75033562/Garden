using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
[RequireComponent(typeof(LayoutElement))]
[ExecuteInEditMode]
public class AutoSizeInputFieldWidth : MonoBehaviour
{
    private LayoutElement m_layoutElement;
    private InputField m_inputField;
    private RectTransform m_rectTransform;

    void Awake()
    {
        m_layoutElement = GetComponent<LayoutElement>();
        m_rectTransform = GetComponent<RectTransform>();
        m_inputField = GetComponent<InputField>();
        m_inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string value)
    {
        var genSettings = m_inputField.textComponent.GetGenerationSettings(Vector2.zero);
        float width = m_inputField.textComponent.cachedTextGeneratorForLayout.GetPreferredWidth(value, genSettings);
        m_layoutElement.preferredWidth = width / m_inputField.textComponent.pixelsPerUnit - 
                                            m_inputField.textComponent.rectTransform.sizeDelta.x;
        LayoutRebuilder.MarkLayoutForRebuild(m_rectTransform);
    }
}
