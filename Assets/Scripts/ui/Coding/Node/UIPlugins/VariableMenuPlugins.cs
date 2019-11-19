using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VariableMenuPlugins : DataMenuPlugins
{
	protected override void Awake()
	{
		base.Awake();
		m_Type = BlockVarType.Variable;
	}
}
