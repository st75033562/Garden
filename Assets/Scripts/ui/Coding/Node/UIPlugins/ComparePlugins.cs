using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CompareOp
{
	Equal,// ==
	NotEqual,// !=
	Greater,// >
	Less,// <
	GreaterEqual,// >=
	LessEqual,// <=
}

public class ComparePlugins : DownMenuPlugins
{
	CompareOp m_Flag;

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
		UpdateCompare();
    }
	
	public void UpdateCompare()
	{
		if ("down_menu_==" == m_TextKey)
		{
			m_Flag = CompareOp.Equal;
        }
		else if("down_menu_!=" == m_TextKey)
		{
			m_Flag = CompareOp.NotEqual;
		}
		else if ("down_menu_>" == m_TextKey)
		{
			m_Flag = CompareOp.Greater;
		}
		else if ("down_menu_<" == m_TextKey)
		{
			m_Flag = CompareOp.Less;
		}
		else if ("down_menu_>=" == m_TextKey)
		{
			m_Flag = CompareOp.GreaterEqual;
		}
		else if ("down_menu_<=" == m_TextKey)
		{
			m_Flag = CompareOp.LessEqual;
		}
	}

	public string Compare(string params1, string params2)
	{
		float floatValue1, floatValue2;
        bool boolValue;
        bool result = false;

        int numbers = 0;
        if (float.TryParse(params1, out floatValue1))
        {
            ++numbers;
        }
        else if (bool.TryParse(params1, out boolValue))
        {
            floatValue1 = boolValue ? 1 : 0;
            ++numbers;
        }

        if (float.TryParse(params2, out floatValue2))
        {
            ++numbers;
        }
        else if (bool.TryParse(params2, out boolValue))
        {
            floatValue2 = boolValue ? 1 : 0;
            ++numbers;
        }

        if (numbers == 2)
		{
			switch (m_Flag)
			{
				case CompareOp.Equal:
                    result = floatValue1 == floatValue2;
					break;
				case CompareOp.NotEqual:
                    result = floatValue1 != floatValue2;
					break;
				case CompareOp.Greater:
                    result = floatValue1 > floatValue2;
					break;
				case CompareOp.Less:
                    result = floatValue1 < floatValue2;
					break;
				case CompareOp.GreaterEqual:
                    result = floatValue1 >= floatValue2;
					break;
				case CompareOp.LessEqual:
                    result = floatValue1 <= floatValue2;
					break;
			}
		}
        else
		{
			int mNum = params1.CompareTo(params2);
			switch (m_Flag)
			{
				case CompareOp.Equal:
                    result = mNum == 0; 
					break;
				case CompareOp.NotEqual:
                    result = mNum != 0;
					break;
				case CompareOp.Greater:
                    result = mNum > 0;
					break;
				case CompareOp.Less:
                    result = mNum < 0;
					break;
				case CompareOp.GreaterEqual:
                    result = mNum >= 0;
					break;
				case CompareOp.LessEqual:
                    result = mNum <= 0;
					break;
			}
		}
        return result ? "true" : "false";
	}

	protected override void OnInput(string str)
	{
		base.OnInput(str);

		ChangePluginsText(str);
		UpdateCompare();
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
		UpdateCompare();
	}
}
