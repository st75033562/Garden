using System.Collections;

public class ARGetMarkerIdsBlock : BlockBehaviour
{
	ListMenuPlugins m_DownMenu;

	protected override void Start()
	{
		base.Start();
		m_DownMenu = GetComponentInChildren<ListMenuPlugins>();
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
		string listName = m_DownMenu.GetMenuValue();
        var listData = CodeContext.variableManager.get<ListData>(listName);
        if (listData != null)
        {
            var markerIds = CodeContext.arSceneManager.GetMarkerIds();
            for (int i = 0; i < markerIds.Count; ++i)
            {
                string markerId = markerIds[i].ToString();
                if(!listData.contains(markerId))
                {
                    listData.add(markerId);
                }
            }
        }

        yield break;
	}
}
