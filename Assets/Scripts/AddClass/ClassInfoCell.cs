using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Google.Protobuf;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class ClassInfoCell : ScrollCell {
    [SerializeField]
    private Text className;
    [SerializeField]
    private Text classInfo;
    [SerializeField]
    private Text TeacherName;
    [SerializeField]
    private Text studentCount;
    [SerializeField]
    private GameObject addButton;
    [SerializeField]
    private GameObject applied;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private RaceLamp raceLamp;

    private ClassInfo banjiInfo;
    private int popupId;
    

    public override void configureCellData()
    {
        ClassInfoData classInfoData = (ClassInfoData)DataObject;
        if(classInfoData == null)
            return;
        raceLamp.ResetPostion();
        banjiInfo = classInfoData.banji;

        className.text = banjiInfo.m_Name;
        classInfo.text = banjiInfo.m_Description;
        icon.sprite = ClassIconResource.GetIcon((int)banjiInfo.m_IconID);
        TeacherName.text = banjiInfo.teacherInfo.nickName;
        studentCount.text = "class_student_count".Localize(banjiInfo.studentsInfos.Count.ToString ());
        if(UserManager.Instance.ClassList.Find (banji => banji.m_ID == banjiInfo.m_ID) == null) {
            ChangeAddState (true);
        } else {
            ChangeAddState (false);
        }
    }

    void ChangeAddState (bool addOpen) {
        if(addOpen) {
            addButton.SetActive (true);
            applied.SetActive (false);
        } else {
            addButton.SetActive (false);
            applied.SetActive (true);
        }
    }

    public void OnClickAddClass ()
    {
        A8_Class_Info classInfo = new A8_Class_Info ();
        classInfo.ClassId = banjiInfo.m_ID;
        classInfo.TeacherId = banjiInfo.teacherId;

        Mail mail = new Mail ();
        mail.SenderId = UserManager.Instance.AccountId;
        mail.ReceiverId = banjiInfo.teacherId;
        mail.MailType = (uint)Mail_Type.MailClassReq;
        mail.Extension = classInfo.ToByteString ();

        CMD_Send_Mail_r_Parameters mail_r = new CMD_Send_Mail_r_Parameters ();
        mail_r.SendMail = mail;
        popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdSendMailR, mail_r.ToByteString(), OnSentMail);
    }

    void OnSentMail (Command_Result res, ByteString content)
    {
        if (res == Command_Result.CmdNoError)
        {
            CMD_Send_Mail_a_Parameters main_a = CMD_Send_Mail_a_Parameters.Parser.ParseFrom(content);
            Mail mail = main_a.SentMail;
            if (mail.MailType == (uint)Mail_Type.MailClassReq)
            {
                A8_Class_Info classInfo = A8_Class_Info.Parser.ParseFrom(mail.Extension);
                ClassInfo banjin = UserManager.Instance.GetClass(classInfo.ClassId);
                if (banjin == null)
                {
                    banjin = new ClassInfo();
                    banjin.m_ID = classInfo.ClassId;
                    banjin.m_ClassStatus = ClassInfo.Status.Applied_Status;
                    UserManager.Instance.ClassList.Add(banjin);
                }
            }
            ChangeAddState(false);
        }
        else
        {
            Debug.LogError("" + res);
        }

        PopupManager.Close(popupId);
    }
}