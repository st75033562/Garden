using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditDataDialogResult
{
    public string name;
    public bool global;
    public GlobalVarOwner globalVarOwner;

    public NameScope scope
    {
        get { return global ? NameScope.Global : NameScope.Local; }
    }

    public static UIEditDataDialogResult Parse(string input)
    {
        return JsonUtility.FromJson<UIEditDataDialogResult>(input);
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

public class UIEditDataDialog : UIEditInputDialog
{
    public Toggle m_globalToggle;
    public GameObject m_globalToggleContainer;
    public Toggle m_writerGameboardToggle;
    public Toggle m_writerRobotToggle;
    public GameObject m_writerContainer;
    public GameObject m_writerText;

    public override void Init()
    {
        base.Init();
        ShowGlobalFlag(false);
        m_globalToggle.onValueChanged.AddListener(OnGlobalChanged);

        m_writerGameboardToggle.onValueChanged.AddListener(isOn => OnWriterChanged(m_writerGameboardToggle, isOn));
        m_writerRobotToggle.onValueChanged.AddListener(isOn => OnWriterChanged(m_writerRobotToggle, isOn));
    }

    public override void OpenDialog()
    {
        base.OpenDialog();

        m_globalToggle.isOn = false;
        m_writerGameboardToggle.isOn = true;
        m_writerRobotToggle.isOn = true;
    }

    public override void CloseDialog()
    {
        base.CloseDialog();
        ShowGlobalFlag(false);
    }

    // if global flag is not displayed, then all data is local
    public void ShowGlobalFlag(bool visible)
    {
        m_globalToggleContainer.SetActive(visible);
    }

    void OnGlobalChanged(bool isOn)
    {
        m_writerContainer.SetActive(isOn);
        m_writerText.SetActive(isOn);
    }

    void OnWriterChanged(Toggle toggle, bool isOn)
    {
        if (!m_writerGameboardToggle.isOn && !m_writerRobotToggle.isOn)
        {
            toggle.isOn = true;
            PopupManager.Notice("ui_error_at_least_one_writer_must_be_selected".Localize());
        }
    }

    protected override string GetCallbackResult()
    {
        var result = new UIEditDataDialogResult();
        result.name = m_InputText.text;
        result.global = m_globalToggle.isOn && m_globalToggleContainer.activeSelf;

        if (result.global)
        {
            if (m_writerRobotToggle.isOn && m_writerGameboardToggle.isOn)
            {
                result.globalVarOwner = GlobalVarOwner.All;
            }
            else if (m_writerGameboardToggle.isOn)
            {
                result.globalVarOwner = GlobalVarOwner.Gameboard;
            }
            else
            {
                result.globalVarOwner = GlobalVarOwner.Robot;
            }
        }
        else
        {
            result.globalVarOwner = GlobalVarOwner.Robot;
        }
        return result.ToString();
    }
}
