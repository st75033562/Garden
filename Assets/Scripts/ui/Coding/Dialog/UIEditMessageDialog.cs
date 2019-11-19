using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditMessageDialogResult
{
    public string name;
    public bool global;
    // if empty, then for all robots
    public int[] targetRobots;

    public NameScope scope
    {
        get { return global ? NameScope.Global : NameScope.Local; }
    }

    public static UIEditMessageDialogResult Parse(string input)
    {
        return JsonUtility.FromJson<UIEditMessageDialogResult>(input);
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

public class UIEditMessageDialog : UIEditInputDialog
{
    public Toggle m_globalToggle;
    public GameObject m_globalToggleContainer;

    public GameObject m_targetRobots;
    public GameObject m_robotGroup;
    public Transform m_robotContainer;
    public GameObjectPool m_robotPool;
    public Toggle m_allRobotsToggle;

    private readonly List<int> m_robotIndices = new List<int>();
    private bool m_showRobotTargets;
    private int m_robotNum;

    public override void Init()
    {
        base.Init();
        OnAllRobotsChanged(m_allRobotsToggle.isOn);
        ShowGlobalFlag(false);
        ShowRobotTargets(false);
    }

    public override void OpenDialog()
    {
        base.OpenDialog();

        m_globalToggle.isOn = false;
        m_allRobotsToggle.isOn = true;

        m_robotIndices.Clear();
        if (m_showRobotTargets)
        {
            m_robotPool.DeallocateAll();
            for (int i = 0; i < m_robotNum; ++i)
            {
                var index = m_robotPool.Allocate<BotIndexToggle>();
                index.index = i;
                index.isOn = false;
                index.transform.SetParent(m_robotContainer);
                index.transform.SetAsLastSibling();
            }
            m_robotPool.Shrink();
        }
    }

    public override void CloseDialog()
    {
        base.CloseDialog();

        ShowGlobalFlag(false);
        ShowRobotTargets(false);
    }

    // if global flag is not displayed, then all data is local
    public void ShowGlobalFlag(bool visible)
    {
        m_globalToggleContainer.SetActive(visible);
    }

    public void ShowRobotTargets(bool visible)
    {
        m_showRobotTargets = visible;
    }

    public void SetRobotNum(int robotNum)
    {
        m_robotNum = Mathf.Max(1, robotNum);
    }

    public void OnGlobalChanged(bool on)
    {
        m_targetRobots.SetActive(on && m_showRobotTargets);
        InputValueChanged();
    }

    public void OnAllRobotsChanged(bool on)
    {
        m_robotContainer.gameObject.SetActive(!on);
        m_robotGroup.SetActive(!on);
        InputValueChanged();
    }

    public void OnRobotIndexClicked(BotIndexToggle toggle)
    {
        if (toggle.isOn)
        {
            toggle.isOn = false;
            m_robotIndices.Remove(toggle.index);
        }
        else
        {
            toggle.isOn = true;
            m_robotIndices.Add(toggle.index);
        }

        InputValueChanged();
    }

    protected override bool ValidateInput()
    {
        bool valid = base.ValidateInput();
        if (valid && m_robotContainer.gameObject.activeInHierarchy)
        {
            valid = m_robotIndices.Count > 0;
        }
        return valid;
    }

    protected override string GetCallbackResult()
    {
        var result = new UIEditMessageDialogResult();
        result.name = m_InputText.text;
        result.global = m_globalToggle.isOn && m_globalToggleContainer.activeSelf;
        if (m_robotContainer.gameObject.activeInHierarchy)
        {
            result.targetRobots = m_robotIndices.ToArray();
        }
        else
        {
            result.targetRobots = new int[0];
        }
        return result.ToString();
    }
}
