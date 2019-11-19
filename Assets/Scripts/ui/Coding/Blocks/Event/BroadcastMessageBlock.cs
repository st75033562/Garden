using System.Collections;

public class BroadcastMessageBlock : BlockBehaviour
{
	DownMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
	}

	public override IEnumerator ActionBlock(ThreadContext context)
	{
		string message = m_Menu.GetMenuValue();
        CodeContext.messageManager.broadcast(message);

        yield break;
	}
}
