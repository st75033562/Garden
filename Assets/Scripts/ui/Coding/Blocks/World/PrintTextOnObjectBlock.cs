using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintTextOnObjectBlock : BlockBehaviour
{
    private DataMenuPlugins m_dataMenu;
    private SelectColorPlugins m_colorPlugin;

    protected override void Start()
    {
        base.Start();

        m_dataMenu = GetComponentInChildren<DataMenuPlugins>();
        m_colorPlugin = GetComponentInChildren<SelectColorPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        int size;
        int.TryParse(slotValues[1], out size);

        int objectId = CodeContext.variableManager.getVarInt(m_dataMenu.GetMenuValue());
        CodeContext.worldApi.PrintTextOnObject(objectId, slotValues[0], size, m_colorPlugin.color);
    }
}
