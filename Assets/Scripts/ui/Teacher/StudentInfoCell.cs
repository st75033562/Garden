using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentInfoCellData{
    public ClassRequestInfo classRequestInfo;
    public MemberInfo memberInfo;
    public StuManagerOperation operationType;
}
public class StudentInfoCell : ScrollCell {
    public Button cellBtn;
    public Image avatarIcon;
    public GameObject applyMark;
    public Text textName;
    public UITeacherStudentManager m_Manager;
    public GameObject operationMark;

    private StudentInfoCellData studentInfo;
    public override void configureCellData() {
        studentInfo = (StudentInfoCellData)DataObject;
        if(studentInfo.classRequestInfo != null) {  //学生请求
            
            applyMark.SetActive(true);
            avatarIcon.sprite = UserIconResource.GetUserIcon((int)studentInfo.classRequestInfo.m_IconID);
            textName.text = studentInfo.classRequestInfo.m_NickName;
        } else {
            cellBtn.interactable = false;
            applyMark.SetActive(false);
            avatarIcon.sprite = UserIconResource.GetUserIcon((int)studentInfo.memberInfo.iconId);
            textName.text = studentInfo.memberInfo.nickName;
        }

        UpdateOpertion();
    }

    public void OnCLickCell() {
        if(studentInfo.operationType == StuManagerOperation.NONE) {
            PopupManager.YesNo("ui_accept_application".Localize(), () => {
                m_Manager.AgreeJoin(studentInfo.classRequestInfo.m_MailID);
            },
            () => {
                m_Manager.RefuseJoin(studentInfo.classRequestInfo.m_MailID);
            },
            "class_accept_application".Localize(), "class_refuse_application".Localize());
        } else if(studentInfo.operationType == StuManagerOperation.DELETE) {
            m_Manager.KickStudent(studentInfo.memberInfo.userId);
        }
    }

    public void UpdateOpertion() {
        if(studentInfo == null) {
            return;
        }
        if(studentInfo.operationType == StuManagerOperation.NONE) {
            cellBtn.interactable = studentInfo.classRequestInfo != null;
            operationMark.SetActive(false);
        } else {
            if(studentInfo.classRequestInfo != null) {
                cellBtn.interactable = false;
            } else {
                cellBtn.interactable = true;
                operationMark.SetActive(true);
            }
        }
    }
}
