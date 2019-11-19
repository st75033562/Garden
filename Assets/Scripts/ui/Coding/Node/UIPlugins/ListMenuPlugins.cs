using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListMenuPlugins : DataMenuPlugins
{
	protected override void Awake()
	{
		base.Awake();
		m_Type = BlockVarType.List;
	}
}
