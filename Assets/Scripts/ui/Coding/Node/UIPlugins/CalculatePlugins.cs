using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum CalculateEnum
{
	Op_Plus,// +
	Op_Minus,// -
	Op_Multiply,// *
	Op_Divide,// /
}

public class CalculatePlugins : DownMenuPlugins
{
	CalculateEnum m_Flag;

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        UpdateCalculate();
    }

	public void UpdateCalculate()
	{
		if("down_menu_+" == m_TextKey)
		{
			m_Flag = CalculateEnum.Op_Plus;
		}
		else if("down_menu_-" == m_TextKey)
		{
			m_Flag = CalculateEnum.Op_Minus;
		}
		else if ("down_menu_*" == m_TextKey)
		{
			m_Flag = CalculateEnum.Op_Multiply;
		}
		else if ("down_menu_/" == m_TextKey)
		{
			m_Flag = CalculateEnum.Op_Divide;
		}
	}

	public string Calculate(string params1, string params2)
	{
		float mParams1;
		if(!float.TryParse(params1, out mParams1))
		{
			return "-1";
		}
		float mParams2;
		if (!float.TryParse(params2, out mParams2))
		{
			return "-1";
		}

		float mReuslt = 0.0f;
		switch (m_Flag)
		{
			case CalculateEnum.Op_Plus:
				{
					mReuslt = mParams1+mParams2;
				}
				break;
			case CalculateEnum.Op_Minus:
				{
					mReuslt = mParams1 - mParams2;
				}
				break;
			case CalculateEnum.Op_Multiply:
				{
					mReuslt = mParams1 * mParams2;
				}
				break;
			case CalculateEnum.Op_Divide:
				{
					if(0 == mParams2)
					{
						mReuslt = 0;
                    }
					else
					{
						mReuslt = mParams1 / mParams2;
					}
				}
				break;
		}

		return mReuslt.ToString();
	}
	protected override void OnInput(string str)
	{
		base.OnInput(str);

		ChangePluginsText(str);
		UpdateCalculate();
	}

	public override Save_PluginsData GetPluginSaveData()
	{
		Save_PluginsData tSave = base.GetPluginSaveData();
		tSave.PluginTextValue = m_TextKey;
		return tSave;
	}

	public override void LoadPluginSaveData(Save_PluginsData save)
	{
		base.LoadPluginSaveData(save);
		m_TextKey = save.PluginTextValue;
		SetPluginsText(m_TextKey);
		UpdateCalculate();
	}
}
