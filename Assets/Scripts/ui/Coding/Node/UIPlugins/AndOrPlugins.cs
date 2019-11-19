using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum AndOrEnum
{
	AndOr_And,
	AndOr_Or,
}

public class AndOrPlugins : DownMenuPlugins
{
	AndOrEnum m_AndOr;

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
		UpdateAndOr();
    }

	public void UpdateAndOr()
	{
		if("down_menu_and" == m_TextKey)
		{
			//m_Text.text = "and";
			m_AndOr = AndOrEnum.AndOr_And;
        }
		else if("down_menu_or" == m_TextKey)
		{
			//m_Text.text = "or";
			m_AndOr = AndOrEnum.AndOr_Or;
        }
	}

	public string Action(string params1, string params2)
	{
        bool exp1 = BlockUtils.ParseBool(params1);
        bool exp2 = BlockUtils.ParseBool(params2);

        bool result = m_AndOr == AndOrEnum.AndOr_And ? (exp1 && exp2) : (exp1 || exp2);
        return result ? "true" : "false";
	}

	protected override void OnInput(string str)
	{
		base.OnInput(str);

        ChangePluginsText(str);
		UpdateAndOr();
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
		UpdateAndOr();
	}
}
