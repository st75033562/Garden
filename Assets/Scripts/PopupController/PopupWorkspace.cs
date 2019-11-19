using UnityEngine;

public class PopupWorkspace : PopupController
{
    public CodePanelManager m_codePanelManager;
    public Canvas m_SimulatorCanvas;
    public Canvas m_ARCanvas;
    public GuideController m_guideController;

    protected override void Start()
    {
        base.Start();

        m_guideController.Initialize();
        StartCoroutine(m_codePanelManager.Init((CodeSceneArgs)payload, false));
        m_codePanelManager.onExit = Close;
    }

    protected override void SetBaseSortingOrder(int order)
    {
        // only 1 canvas is visible at the same time
        m_ARCanvas.sortingOrder = order;
        m_SimulatorCanvas.sortingOrder = order;
        m_codePanelManager.Workspace.m_RootCanvas.sortingOrder = order;
    }
}
