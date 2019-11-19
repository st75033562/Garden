using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ARSetMarkerObjectBlock : BlockBehaviour
{
    ARObjectSelectPlugins m_ObjectSelectPlugin;

    protected override void Start()
    {
        base.Start();
        m_ObjectSelectPlugin = GetComponentInChildren<ARObjectSelectPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int markerId;
        if (int.TryParse(slotValues[0], out markerId) && markerId >= 0)
        {
            CodeContext.arSceneManager.SetModel(markerId, m_ObjectSelectPlugin.objectId);
        }
    }
}
