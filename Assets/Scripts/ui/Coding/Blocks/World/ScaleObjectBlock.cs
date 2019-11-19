using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObjectBlock : BlockBehaviour
{
    private DataMenuPlugins m_variableMenu;

    protected override void Start()
    {
        base.Start();

        m_variableMenu = GetComponentInChildren<DataMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        var objectVar = CodeContext.variableManager.get<VariableData>(m_variableMenu.GetMenuValue());
        var objectId = objectVar != null ? (int)objectVar.getValue() : 0;

        float x, y, z;
        if (objectId != 0 &&
            float.TryParse(slotValues[0], out x) &&
            float.TryParse(slotValues[1], out y) &&
            float.TryParse(slotValues[2], out z))
        {
            CodeContext.worldApi.SetObjectScale(objectId, new Vector3(x, y, z));
        }
    }

}
