using System;

public static class TaskCommon
{
    public const string TaskFileName = "taskInfo.pro";
    public const string c_CommentFileName = "comm";

    public static string GetTaskPath(uint classId, uint taskId, uint userId)
    {
        return "/download/class_v3/" + classId + "/" + taskId + "/" + userId;
    }

    public static string GetTemplate(uint userId, uint level, string name, TaskTemplateType type) {
        string str = "";
        if(type == TaskTemplateType.System) {
            str = "system/" + RequestUtils.Base64Encode(level.ToString()) + "/";
        } else {
            str = userId.ToString() + "/" + RequestUtils.Base64Encode(level.ToString()) + "/";
        }

        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            return "/download/tasktemplate_v3/python/"+ str + RequestUtils.Base64Encode(name);
            } else {
            return "/download/tasktemplate_v3/graphy/" + str + RequestUtils.Base64Encode(name);
        }
    }

    public static GetCatalogType GetCatalog(TaskTemplateType type, ScriptLanguage language)
    {
        if (language == ScriptLanguage.Num)
        {
            throw new ArgumentException("language");
        }

        if (language == ScriptLanguage.Visual && type == TaskTemplateType.User)
        {
            return GetCatalogType.TEACHER_TASK;
        }
        else if (language == ScriptLanguage.Visual)
        {
            return GetCatalogType.SYSTEM_TASK;
        }
        else if (language == ScriptLanguage.Python && type == TaskTemplateType.User)
        {
            return GetCatalogType.TEACHER_TASK_PY;
        }
        else
        {
            return GetCatalogType.SYSTEM_TASK_PY;
        }
    }
}
