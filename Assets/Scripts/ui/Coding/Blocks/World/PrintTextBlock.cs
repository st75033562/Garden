using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintTextBlock : BlockBehaviour
{
    private SelectColorPlugins m_colorPlugin;
    private GrayColorPlugins m_bgPlugin;

    protected override void Start()
    {
        base.Start();

        m_colorPlugin = GetComponentInChildren<SelectColorPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
        m_bgPlugin = GetComponentInChildren<GrayColorPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        PrintTextConfig cfg;

        cfg.text = slotValues[0];
        cfg.color = m_colorPlugin.color;
        cfg.bgBrightness = m_bgPlugin.brightness;
        cfg.bgAlpha = m_bgPlugin.alpha;

        float.TryParse(slotValues[1], out cfg.pos.x);
        float.TryParse(slotValues[2], out cfg.pos.y);
        int.TryParse(slotValues[3], out cfg.size);
        float.TryParse(slotValues[4], out cfg.duration);

        CodeContext.textPanel.Print(ref cfg);
    }
}
