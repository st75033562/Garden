using System.Collections;

public class GetMarkerTransformBlock : BlockBehaviour
{
    private ARMarkerSelectPlugins m_markerPlugin;
    private DataMenuPlugins[] m_dataMenuPlugins;

    protected override void Start()
    {
        base.Start();

        m_markerPlugin = GetComponentInChildren<ARMarkerSelectPlugins>();
        m_dataMenuPlugins = GetComponentsInChildren<DataMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var markerId = m_markerPlugin.GetMarkerId();
        var pos = CodeContext.gameboardService.GetMarkerPosition(markerId);
        var rot = CodeContext.gameboardService.GetMarkerRotation(markerId);

        CodeContext.variableManager.setVar(m_dataMenuPlugins[0].GetMenuValue(), pos.x);
        CodeContext.variableManager.setVar(m_dataMenuPlugins[1].GetMenuValue(), pos.y);
        CodeContext.variableManager.setVar(m_dataMenuPlugins[2].GetMenuValue(), pos.z);
        CodeContext.variableManager.setVar(m_dataMenuPlugins[3].GetMenuValue(), rot);

        yield break;
    }
}
