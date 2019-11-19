using System.Collections;
using System.Collections.Generic;

public class ARShowMarkerObjectBlock : BlockBehaviour
{
    DownMenuPlugins m_ShowOrHideMenuPlugin;

    protected override void Start()
    {
        base.Start();

        m_ShowOrHideMenuPlugin = GetComponentInChildren<DownMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);

        int markerId;
        if (int.TryParse(slotValues[0], out markerId) && markerId >= 0)
        {
            string showOption = m_ShowOrHideMenuPlugin.GetMenuValue();
            if ("ar_marker_show" == showOption)
            {
                CodeContext.arSceneManager.ShowMarkerObject(markerId);
            }
            else if ("ar_marker_hide" == showOption)
            {
                CodeContext.arSceneManager.HideMarkerObject(markerId);
            }
        }
    }
}
