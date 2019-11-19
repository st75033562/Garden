using UnityEngine;
using UnityEngine.UI;

public class DeleteNode : MonoBehaviour
{
    public Color m_NormalColor;
    public Color m_DeleteColor;
    public Image m_Background;

    bool m_Active;

    void Awake()
    {
        ShowDeleteFlag(false);
    }

    public bool DeleteFlagVisible
    {
        get { return m_Active; }
    }

    public void ShowDeleteFlag(bool show)
    {
        m_Background.color = show ? m_DeleteColor : m_NormalColor;
        m_Active = show;
    }
}
