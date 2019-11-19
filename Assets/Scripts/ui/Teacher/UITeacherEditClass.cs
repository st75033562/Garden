using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Google.Protobuf;
using System;

public class UITeacherEditClass : PopupController
{
    public class PayLoad {
        public uint classId;
        public ScriptLanguage classType;
        public WorkMode workMode;
        public Action refreshBack;
    }

	public Text m_Title;
	public InputField m_NameInput;
	public InputField m_DescriptionInput;
	public Image m_IconBtn;
	public Text m_ConfirmText;
	public Sprite m_IconBtnDefaultImage;
	public UITeacherSelectClassIcon m_SelectIcon;
    public GameObject m_avatarAddImage;

    int m_IconID;
	uint m_ClassID;
    ScriptLanguage m_ClassType;
    Action m_RefreshBack;
    public enum WorkMode
	{
		CreateNew_Mode,
		ChangeInfo_Mode,
	}

	WorkMode m_WorkMode;
    // Use this for initialization
    protected override void Start()
	{
        base.Start();

        var data = (PayLoad)payload;
        m_WorkMode = data.workMode;
        m_ClassType = data.classType;
        m_RefreshBack = data.refreshBack;
        if(m_WorkMode == WorkMode.CreateNew_Mode) {
            OpenByCreateClass();
        } else {
            OpenByChangeClass(data.classId);
        }

		m_NameInput.placeholder.GetComponent<Text>().text = "class_name_hint".Localize();
		m_DescriptionInput.placeholder.GetComponent<Text>().text = "class_desc_hint".Localize();
		m_ConfirmText.text = "ui_ok".Localize();
		m_IconID = 1;
    }


	public void OpenByCreateClass()
	{
		m_Title.text = "class_add".Localize();

		m_NameInput.text = "";
		m_DescriptionInput.text = "";
		m_IconBtn.sprite = m_IconBtnDefaultImage;
        m_avatarAddImage.SetActive(true);
    }

	public void OpenByChangeClass(uint classID)
	{
		m_Title.text = "my_class".Localize();

		ClassInfo tCurClass = UserManager.Instance.GetClass(classID);
		if (null != tCurClass)
		{
			m_NameInput.text = tCurClass.m_Name;
			m_DescriptionInput.text = tCurClass.m_Description;
			m_IconBtn.sprite = ClassIconResource.GetIcon((int)tCurClass.m_IconID);
			m_ClassID = classID;
			m_IconID = (int)tCurClass.m_IconID;
            m_avatarAddImage.SetActive(false);
        }
	}

	public void ClickConfirm()
	{
		if(string.IsNullOrEmpty(m_NameInput.text))
		{
            PopupManager.Notice("empty_class_name".Localize());
			return;
		}
		switch (m_WorkMode)
		{
			case WorkMode.CreateNew_Mode:
				{
					CreateNewClass();
				}
				break;
			case WorkMode.ChangeInfo_Mode:
				{
					ModifyClassInfo();
				}
				break;
		}
	}

	void CreateNewClass()
	{
		CMD_Create_Class_r_Parameters tNewClassRequest = new CMD_Create_Class_r_Parameters();
		tNewClassRequest.CreateInfo = new A8_Class_Info();
		tNewClassRequest.CreateInfo.ClassName = m_NameInput.text;
		tNewClassRequest.CreateInfo.ClassDescription = m_DescriptionInput.text;
		tNewClassRequest.CreateInfo.ClassInconId = (uint)m_IconID;
        tNewClassRequest.CreateInfo.ClassProjectType = (uint)m_ClassType;

        int popId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdCreateClassR, tNewClassRequest.ToByteString(), (res, content) => {
            PopupManager.Close(popId);
            if(res == Command_Result.CmdNoError) {
                CMD_Create_Class_a_Parameters tClassInfo = CMD_Create_Class_a_Parameters.Parser.ParseFrom(content);
                ClassInfo tNewClassData = new ClassInfo();
                tNewClassData.m_ID = tClassInfo.ClassInfo.ClassId;
                tNewClassData.m_Name = tClassInfo.ClassInfo.ClassName;
                tNewClassData.m_Description = tClassInfo.ClassInfo.ClassDescription;
                tNewClassData.m_IconID = tClassInfo.ClassInfo.ClassInconId;
                tNewClassData.languageType = (ScriptLanguage)tClassInfo.ClassInfo.ClassProjectType;
                tNewClassData.m_createTime = TimeUtils.FromEpochSeconds((long)tClassInfo.ClassInfo.ClassCreateTime);
                tNewClassData.m_ClassStatus = ClassInfo.Status.Create_Status;
                UserManager.Instance.ClassList.Add(tNewClassData);

                m_RefreshBack();
                Close();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });

	}

	void ModifyClassInfo()
	{
		CMD_Update_Classinfo_r_Parameters tModifyClass = new CMD_Update_Classinfo_r_Parameters();
		tModifyClass.UpdateClass = new A8_Class_Info();
		tModifyClass.UpdateClass.ClassId = m_ClassID;
		tModifyClass.UpdateClass.ClassName = m_NameInput.text;
		tModifyClass.UpdateClass.ClassDescription = m_DescriptionInput.text;
		tModifyClass.UpdateClass.ClassInconId = (uint)m_IconID;

        int popId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdUpdateClassinfoR, tModifyClass.ToByteString(), (res, content) => {
            PopupManager.Close(popId);
            if(res == Command_Result.CmdNoError) {
                CMD_Update_Classinfo_a_Parameters tRt = CMD_Update_Classinfo_a_Parameters.Parser.ParseFrom(content);
                ClassInfo tCurClass = UserManager.Instance.GetClass(tRt.UpdatedClass.ClassId);
                if(null != tCurClass) {
                    tCurClass.UpdateInfo(tRt.UpdatedClass);
                }
                m_RefreshBack();
                Close();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

	public void ClickSelectIcon()
	{
		m_SelectIcon.SetActive(true, IconSelectCallBack);
    }

	public void IconSelectCallBack(int id)
	{
		m_IconID = id;
		m_IconBtn.sprite = ClassIconResource.GetIcon(m_IconID);
        m_avatarAddImage.SetActive(false);
    }

}
