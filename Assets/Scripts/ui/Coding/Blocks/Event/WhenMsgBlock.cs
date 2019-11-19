using UnityEngine;
using System.Collections;

public class WhenMsgBlock : LoopMainBlock
{
	DownMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		m_Menu = GetComponentInChildren<DownMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

	public override IEnumerator ActionBlock(ThreadContext context)
	{
        while (!CodeContext.messageHandler.IsBroadcasted(m_Menu.GetMenuValue()))
        {
            yield return null;
        }
    }
}
