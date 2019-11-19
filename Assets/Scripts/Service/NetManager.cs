using UnityEngine;
using System.Linq;
using Google.Protobuf;
using System;
using System.Collections.Generic;

public class NetManager : Singleton<NetManager>
{
    public List<TaskTemplate> taskPools = new List<TaskTemplate>();
    public List<TaskTemplate> sysTaskPools = new List<TaskTemplate>();

    private Action<Command_Result> allClassInfosAction;
    public void GetAllClassInfos(Action<Command_Result> action, Project_Language_Type type)
    {
        allClassInfosAction = action;
        CMD_Get_Classinfo_r_Parameters classInfo_r = new CMD_Get_Classinfo_r_Parameters();
        classInfo_r.AttendClass = 1;
        classInfo_r.AppliedClass = 1;
        classInfo_r.CreatedClass = 1;
        classInfo_r.ReqProjectType = (uint)type;

        SocketManager.instance.send(Command_ID.CmdGetClassinfoR, classInfo_r.ToByteString(), ReceiveAllClassInfo);
    }

    void ReceiveAllClassInfo(Command_Result res, ByteString content)
    {
        if (res == Command_Result.CmdNoError)
        {
            CMD_Get_Classinfo_a_Parameters classInfo_a = CMD_Get_Classinfo_a_Parameters.Parser.ParseFrom(content);
            UserManager.Instance.ClassList.RemoveAll(x=> { return x.languageType == Preference.scriptLanguage; });
            foreach (A8_Class_Info class_info in classInfo_a.AttendClassList)
            {
                ClassInfo banji = new ClassInfo();
                banji.UpdateInfo(class_info);
                banji.m_ClassStatus = ClassInfo.Status.Attend_Status;
                UserManager.Instance.ClassList.Add(banji);
            }
            foreach (A8_Class_Info class_info in classInfo_a.AppliedClassList)
            {
                ClassInfo banji = new ClassInfo();
                banji.UpdateInfo(class_info);
                banji.m_ClassStatus = ClassInfo.Status.Applied_Status;
                UserManager.Instance.ClassList.Add(banji);
            }
            foreach(A8_Class_Info class_info in classInfo_a.CreatedClassList) {
                ClassInfo banji = new ClassInfo();
                banji.UpdateInfo(class_info);
                banji.m_ClassStatus = ClassInfo.Status.Create_Status;
                UserManager.Instance.ClassList.Add(banji);
            }
            allClassInfosAction(Command_Result.CmdNoError);
        }
        else
        {
            allClassInfosAction(res);
        }
        allClassInfosAction = null;
    }


    public GetTemplateTasksRequest GetTeacherTaskPool(Action<List<TaskTemplate>> callback)
    {
        taskPools.Clear();
        var request = new GetUserTemplateTasksRequest();
        request.userId = UserManager.Instance.UserId;
        request.language = Preference.scriptLanguage;
        request.Success(res => {
                taskPools = res;
            })
            .Finally(() => {
                if (callback != null)
                {
                    callback(taskPools);
                }
            })
            .Execute();
        return request;
    }

    public bool SameNameInTaskPool(string taskName)
    {
        return taskPools.Any(x => x.name == taskName);
    }

    public void CoverOrAddToTaskPool(TaskTemplate task)
    {
        taskPools.Remove(x => x.id == task.id);
        taskPools.Add(task);
    }

    public GetTemplateTasksRequest GetSystemTaskPool(Action<List<TaskTemplate>> callback)
    {
        sysTaskPools.Clear();
        var request = new GetSystemTemplateTasksRequest();
        request.language = Preference.scriptLanguage;
        request.Success(res => {
                sysTaskPools = res;
            })
            .Finally(() => {
                if (callback != null)
                {
                    callback(sysTaskPools);
                }
            })
            .Execute();
        return request;
    }

    public bool SameNameInSysPool(string taskName)
    {
        return sysTaskPools.Any(x => x.name == taskName);
    }

    public void CoverOrAddToSysPool(TaskTemplate info)
    {
        sysTaskPools.Remove(x => x.id == info.id);
        sysTaskPools.Add(info);
    }

    public void Run()
    {
        SocketManager.instance.setListener(Command_ID.CmdNewMailNotify, OnNewMailReceived);
        SocketManager.instance.setListener(Command_ID.CmdCoinsChangeNotify, OnCoinsValueChange);
    }

    public void Stop()
    {
        SocketManager.instance.removeListener(Command_ID.CmdNewMailNotify, OnNewMailReceived);
        SocketManager.instance.removeListener(Command_ID.CmdCoinsChangeNotify, OnCoinsValueChange);
    }

    private void OnNewMailReceived(Command_Result res, ByteString data)
    {
        var response = CMD_New_Mail_Notify_Parameters.Parser.ParseFrom(data);
        var type = (Mail_Type)response.NewMail.MailType;
        switch (type)
        {
        case Mail_Type.MailLeaveClass:
            UserManager.Instance.AddKickNotification(response.NewMail.ToKickNotification());
            break;

        default:
            Debug.LogError("unhandled mail type: " + type);
            break;
        }
    }

    private void OnCoinsValueChange(Command_Result res, ByteString data) 
    {
        var response = CMD_Coins_Change_Notify_Parameters.Parser.ParseFrom(data);
        UserManager.Instance.Coin = (int)response.Coins;
    }

    public void RemoveKickNotifications(IEnumerable<KickNotification> notifs, Action<bool> done)
    {
        var request = new CMD_Del_Mail_r_Parameters();
        request.MailIds.AddRange(notifs.Select(x => x.id));

        SocketManager.instance.send(Command_ID.CmdDelMailR, request, (res, data) => {
                if (res == Command_Result.CmdNoError)
                {
                    var response = CMD_Del_Mail_a_Parameters.Parser.ParseFrom(data);
                    foreach (var id in response.MailIds)
                    {
                        UserManager.Instance.RemoveKickNotification(id);
                    }
                }

                if (done != null)
                {
                    done(res == Command_Result.CmdNoError);
                }
            });
    }
}
