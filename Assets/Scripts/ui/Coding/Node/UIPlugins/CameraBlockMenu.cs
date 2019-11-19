using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBlockMenu : BlockBehaviour
{

    private CameraMenu m_resourceMenu;
    private DataMenuPlugins m_variableMenu;

    protected override void Start()
    {
        base.Start();

        m_resourceMenu = GetComponentInChildren<CameraMenu>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        if (m_resourceMenu != null && m_resourceMenu.cameraManager != null)
        {
            m_resourceMenu.cameraManager.ActivateCamera(m_resourceMenu.assetId);
        }
        yield return null;
    }
}
