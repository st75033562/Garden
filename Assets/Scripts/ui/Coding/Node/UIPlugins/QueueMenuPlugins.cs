using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QueueMenuPlugins : DataMenuPlugins
{
	protected override void Awake()
	{
		base.Awake();
		m_Type = BlockVarType.Queue;
	}
}
