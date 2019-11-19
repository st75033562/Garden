using UnityEngine;
using UnityEngine.UI;

public class UIClassCell : ScrollCell
{
    public Text m_ClassName;
    public GameObject[] m_MaskGos;
    public float m_PressTime;
    public Image m_Icon;
    public Text m_Num;

    public UITeacherMainView m_view;

    public override void configureCellData()
    {
        m_ClassName.text = banjiInfo.m_Name;
        m_Icon.sprite = ClassIconResource.GetIcon((int)banjiInfo.m_IconID);
        m_Num.text = "ui_text_student".Localize() + banjiInfo.studentsInfos.Count.ToString();

        foreach (var mask in m_MaskGos)
        {
            mask.SetActive(m_view.operationType != TeacherMainOperationType.NONE);
        }
    }

    public ClassInfo banjiInfo
    {
        get { return (ClassInfo)DataObject; }
    }

    public void OnClickCell()
    {
        if (m_view.operationType == TeacherMainOperationType.NONE)
        {
            m_view.SelectClass(banjiInfo.m_ID);
        }
        else if (m_view.operationType == TeacherMainOperationType.DELETE)
        {
            m_view.DeleteClass(banjiInfo.m_ID);
        }
        else if (m_view.operationType == TeacherMainOperationType.EDIT)
        {
            m_view.EditClass(banjiInfo.m_ID);
        }
    }
}
