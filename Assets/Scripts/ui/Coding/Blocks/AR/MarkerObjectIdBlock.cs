using Gameboard;
using System.Collections;

public class MarkerObjectIdBlock : BlockBehaviour
{
    private ARMarkerSelectPlugins m_markerPlugin;

    protected override void Start()
    {
        base.Start();
        m_markerPlugin = GetComponentInChildren<ARMarkerSelectPlugins>();
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var id = CodeContext.arSceneManager.GetMarkerObjectId(m_markerPlugin.GetMarkerId());
        retValue.value = id.ToString();
        yield break;
    }
}
