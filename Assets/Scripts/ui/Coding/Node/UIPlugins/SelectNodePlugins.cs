using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public enum SelectNodeEnum
{
	C,
	C_,
	D,
	Eb,
	E,
	F,
	F_,
	G,
	G_,
	A,
	Bb,
	B,
}

public class SelectNodePlugins : DownMenuPlugins
{
	SelectNodeEnum m_Flag;

    protected override void Start()
    {
        UpdateEnum();
        base.Start();
    }

	public void UpdateEnum()
	{
		if (m_TextKey == "down_menu_voice_C")
		{
			m_Flag = SelectNodeEnum.C;
		}
		else if (m_TextKey == "down_menu_voice_C#")
		{
			m_Flag = SelectNodeEnum.C_;
		}
		else if (m_TextKey == "down_menu_voice_D")
		{
			m_Flag = SelectNodeEnum.D;
		}
		else if (m_TextKey == "down_menu_voice_Eb")
		{
			m_Flag = SelectNodeEnum.Eb;
		}
		else if (m_TextKey == "down_menu_voice_E")
		{
			m_Flag = SelectNodeEnum.E;
		}
		else if (m_TextKey == "down_menu_voice_F")
		{
			m_Flag = SelectNodeEnum.F;
		}
		else if (m_TextKey == "down_menu_voice_F#")
		{
			m_Flag = SelectNodeEnum.F_;
		}
		else if (m_TextKey == "down_menu_voice_G")
		{
			m_Flag = SelectNodeEnum.G;
		}
		else if (m_TextKey == "down_menu_voice_G#")
		{
			m_Flag = SelectNodeEnum.G_;
		}
		else if (m_TextKey == "down_menu_voice_A")
		{
			m_Flag = SelectNodeEnum.A;
		}
		else if (m_TextKey == "down_menu_voice_Bb")
		{
			m_Flag = SelectNodeEnum.Bb;
		}
		else if (m_TextKey == "down_menu_voice_B")
		{
			m_Flag = SelectNodeEnum.B;
		}
	}

	protected override void UpdateText()
	{
		switch(m_Flag)
		{
			case SelectNodeEnum.C:
				{
					m_TextKey = "down_menu_voice_C";
                }break;
			case SelectNodeEnum.C_:
				{
					m_TextKey = "down_menu_voice_C#";
				}
				break;
			case SelectNodeEnum.D:
				{
					m_TextKey = "down_menu_voice_D";
				}
				break;
			case SelectNodeEnum.Eb:
				{
					m_TextKey = "down_menu_voice_Eb";
				}
				break;
			case SelectNodeEnum.E:
				{
					m_TextKey = "down_menu_voice_E";
				}
				break;
			case SelectNodeEnum.F:
				{
					m_TextKey = "down_menu_voice_F";
				}
				break;
			case SelectNodeEnum.F_:
				{
					m_TextKey = "down_menu_voice_F#";
				}
				break;
			case SelectNodeEnum.G:
				{
					m_TextKey = "down_menu_voice_G";
				}
				break;
			case SelectNodeEnum.G_:
				{
					m_TextKey = "down_menu_voice_G#";
				}
				break;
			case SelectNodeEnum.A:
				{
					m_TextKey = "down_menu_voice_A";
				}
				break;
			case SelectNodeEnum.Bb:
				{
					m_TextKey = "down_menu_voice_Bb";
				}
				break;
			case SelectNodeEnum.B:
				{
					m_TextKey = "down_menu_voice_B";
				}
				break;
		}
        base.UpdateText();
	}

	public override string GetMenuValue()
	{
		string mStr = "";
		mStr = m_Flag.ToString();
		return mStr;
	}

	public int GetNoteID()
	{
		return (int)m_Flag;
	}

	protected override void OnInput(string str)
	{
		base.OnInput(str);
		UpdateEnum();
	}

	public override Save_PluginsData GetPluginSaveData()
	{
		Save_PluginsData tSave = base.GetPluginSaveData();
		tSave.PluginIntValue = (int)m_Flag;
		return tSave;
	}

	public override void LoadPluginSaveData(Save_PluginsData save)
	{
		base.LoadPluginSaveData(save);
		m_Flag = (SelectNodeEnum)save.PluginIntValue;
		UpdateText();
	}
}
