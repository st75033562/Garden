using Google.Protobuf;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum TeacherClassTab
{
    Task,
    Template,
    Student
}

public class UITeacherClassMain : PopupController
{
    public UITeacherTask m_Task;
    public UITeacherStudentManager m_Student;
    public UITeacherTaskPool m_TaskPool;
    public UISystemTaskPool m_SystemPool;
    public GameObject m_template;

    public GameObject[] m_tabPages;
    public Toggle[] m_tabToggles;

    int m_currentTab = -1;
    int m_maskCount;
    int m_maskId;
    private bool loadedPool;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    void ShowMask()
    {
        if (m_maskCount == 0)
        {
            m_maskId = PopupManager.ShowMask();
        }
        ++m_maskCount;
    }

    void CloseMask()
    {
        Assert.IsTrue(m_maskCount > 0);
        if (--m_maskCount == 0)
        {
            PopupManager.Close(m_maskId);
        }
    }

    public void SetActiveTab(int tab)
    {
        if (tab < 0 || tab > (int)TeacherClassTab.Student)
        {
            throw new ArgumentOutOfRangeException("tab");
        }

        m_currentTab = tab;
        m_tabToggles[tab].isOn = true;
        for (int i = 0; i < m_tabPages.Length; ++i)
        {
            m_tabPages[i].SetActive(i == tab);
            if (i == (int)TeacherClassTab.Student)
            {
                m_Student.OnActivate(m_tabPages[i].activeSelf);
            }
        }
        if (tab == 1 && !loadedPool) { //模板
            loadedPool = true;
            ShowMask();
            NetManager.instance.GetTeacherTaskPool((data) => {
                NetManager.instance.GetSystemTaskPool((data1) => {
                    CloseMask();
                });
            });
        }
    }

    public void ShowTaskPool()
    {
        m_TaskPool.gameObject.SetActive(true);
    }

    public void ShowSystemPool()
    {
        m_SystemPool.gameObject.SetActive(true);
    }

    public void OpenWindow(bool isPush)
    {
        if (isPush && m_currentTab == -1)
        {
            SetActiveTab(0);
        }
    }

    public void CloseWindow(bool isPush)
    {
        if (!isPush)
        {
            m_currentTab = -1;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        UserManager.Instance.CurClass = null;
    }
}
