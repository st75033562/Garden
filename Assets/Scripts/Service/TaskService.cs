using System;
using System.Collections.Generic;

public class TaskService
{
    private static GetCatalogType ToCatalogType(TaskTemplateType type)
    {
        return type == TaskTemplateType.System ? GetCatalogType.SYSTEM_TASK : GetCatalogType.TEACHER_TASK;
    }

    public void PublishTaskToClass(string taskId, uint classId, Action<Command_Result, TaskInfo> callback)
    {
        var request = new WebRequestData();
        request.blocking = true;
        request.useDefaultErrorHandling = true;
        request.m_SuccessCallBack = (resp, userData) => {
            CMD cmd = CMD.Parser.ParseFrom(resp.bytes);
            if (cmd.Result == Command_Result.CmdNoError)
            {
                var response = CMD_Create_Task_a_Parameters.Parser.ParseFrom(cmd.CmdParameters);
                var taskInfo = new TaskInfo();
                taskInfo.SetValue(response.TaskInfo);
                callback(Command_Result.CmdNoError, taskInfo);
            }
            else
            {
                callback(cmd.Result, null);
            }
        };

        WebRequestManager.Default.PublishTaskFromPools(taskId, classId, request);
    }

    public void CopySystemTemplateToUserPool(string taskId, Action success)
    {
        var request = new WebRequestData();
        request.useDefaultErrorHandling = true;
        request.blocking = true;
        request.m_SuccessCallBack = delegate { success(); };
        WebRequestManager.Default.addsysTemplateToself(taskId, request);
    }
}
