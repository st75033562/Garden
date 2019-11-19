using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetObjectTransformBlock : BlockBehaviour
{
    private DataMenuPlugins m_variableMenu;

    protected override void Start()
    {
        base.Start();

        m_variableMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var objectVar = CodeContext.variableManager.get<VariableData>(m_variableMenu.GetMenuValue());
        var objectId = objectVar != null ? (int)objectVar.getValue() : 0;
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        float x, y, z, theta;
        if (objectId != 0 &&
            float.TryParse(slotValues[0], out x) &&
            float.TryParse(slotValues[1], out y) &&
            float.TryParse(slotValues[2], out z) &&
            float.TryParse(slotValues[3], out theta))
        {
            CodeContext.worldApi.SetObjectPosition(objectId, new Vector3(x, y, z));
            CodeContext.worldApi.SetObjectRotation(objectId, theta);
        }
    }
}

public class SetObjectTransformBlockNew : BlockBehaviour
{
    private DataMenuPlugins m_variableMenu;

    protected override void Start()
    {
        base.Start();

        m_variableMenu = GetComponentInChildren<DataMenuPlugins>(true);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var objectVar = CodeContext.variableManager.get<VariableData>(m_variableMenu.GetMenuValue());
        var objectId = objectVar != null ? (int)objectVar.getValue() : 0;
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        float x, y, z, alpha, bate, theta;
        if (objectId != 0 &&
            float.TryParse(slotValues[0], out x) &&
            float.TryParse(slotValues[1], out y) &&
            float.TryParse(slotValues[2], out z) &&
            float.TryParse(slotValues[3], out alpha) &&
            float.TryParse(slotValues[4], out bate) &&
            float.TryParse(slotValues[5], out theta)
            )
        {
            CodeContext.worldApi.SetObjectPosition(objectId, new Vector3(x, y, z));
            CodeContext.worldApi.SetObjectRotation(objectId, alpha, bate, theta);
        }

    }
}
