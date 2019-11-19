using System;
using UnityEngine;

[ExecuteInEditMode]
public class SceneFadeInOut : SceneAnimation
{
    [SerializeField]
    private CanvasGroup m_canvasGroup;

    [SerializeField]
    private float m_alpha;

    protected override void Awake()
    {
        base.Awake();

        if (!m_canvasGroup)
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (!m_canvasGroup)
            {
                Debug.LogError("No canvas group found");
            }
        }

        if (m_canvasGroup)
        {
            m_alpha = m_canvasGroup.alpha;
        }
    }

    protected override void OnUpdate()
    {
        if (m_canvasGroup)
        {
            m_canvasGroup.alpha = m_alpha;
        }
    }

    [ContextMenu("Setup")]
    public override void Setup()
    {
        base.Setup();

        if (!m_canvasGroup)
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
