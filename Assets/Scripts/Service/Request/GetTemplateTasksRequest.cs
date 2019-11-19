using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class GetTemplateTasksRequest : HttpRequest<List<TaskTemplate>>
{
    private readonly List<ProjectDownloadRequestV3> m_subRequests = new List<ProjectDownloadRequestV3>();
    private ScriptLanguage m_language = ScriptLanguage.Num;

    const string pathSplitChar = "/";
    public override void Abort()
    {
        base.Abort();

        foreach (var req in m_subRequests)
        {
            req.Abort();
        }
        m_subRequests.Clear();
    }

    public ScriptLanguage language
    {
        get { return m_language; }
        set
        {
            if (value == ScriptLanguage.Num)
            {
                throw new ArgumentException("value");
            }
            m_language = value;
        }
    }

    protected override void Validate()
    {
        if (m_language == ScriptLanguage.Num)
        {
            throw new InvalidRequestException("language");
        }
    }

    protected override void OnSuccess(ResponseData response)
    {
        var templates = new List<TaskTemplate>();
        var getTaskCounter = 0;
        var totalTaskCount = 0;

        var multiFile = MultiFileList.Parser.ParseFrom(response.bytes);

        foreach (string key in multiFile.FileLists.Keys)
        {
            var tasks = multiFile.FileLists[key];
            foreach(var task in tasks.FileList_) {
                if((FN_TYPE)task.FnType == FN_TYPE.FnFile || task.PathName.Contains(pathSplitChar)) {
                    continue;
                }
                totalTaskCount ++;
                var template = new TaskTemplate();
                template.id = RequestUtils.Base64Encode(key) + "/" + task.Base64PathName;
                template.name = task.PathName;

                template.level = (TaskCategory)int.Parse(key);
                template.type = type;

                // get the real task info
                GetTemplateTaskInfo(template, ok => {
                    if(ok) {
                        templates.Add(template);
                    }
                    if(++getTaskCounter == totalTaskCount) {
                        SetResult(templates);
                        base.OnSuccess(response);
                    }
                });
            }
        }
    }

    private void GetTemplateTaskInfo(TaskTemplate template, Action<bool> callback)
    {
        var request = new ProjectDownloadRequestV3(requestManager);
        request.basePath = template.id;
        request.userId = UserManager.Instance.UserId;
        request.type = TaskCommon.GetCatalog(type, language);

        request.blocking = blocking;
        request.Success(tRt => {
            for (int i = 0; i < tRt.FileList_.Count; ++i)
            {
                FileNode tCurFile = tRt.FileList_[i];

                //if (language == ScriptLanguage.Visual)
                //{
                //    if (CodeProjectRepository.ProjectFileName == tCurFile.PathName)
                //    {
                //        template.codeProject.code = tCurFile.FileContents.ToByteArray();
                //    }
                //    else if (CodeProjectRepository.LeaveMessageFileName == tCurFile.PathName)
                //    {
                //        template.codeProject.leaveMessageData = tCurFile.FileContents.ToByteArray();
                //    }
                //}

                if (TaskCommon.TaskFileName == tCurFile.PathName)
                {
                    var taskInfo = A8_Task_Info.Parser.ParseFrom(tCurFile.FileContents);
                    template.projectName = taskInfo.TaskProgramName;
                    template.description = taskInfo.TaskDescription;
                    if(taskInfo.TaskAttachInfoNew != null) {
                        template.attachs = taskInfo.TaskAttachInfoNew;
                    }
                    template.createTime = TimeUtils.FromEpochSeconds((long)tCurFile.CreateTime);
                    template.updateTime = TimeUtils.FromEpochSeconds((long)tCurFile.UpdateTime);

                    if (language == ScriptLanguage.Visual)
                    {
                        template.codeProject.name = taskInfo.TaskProgramName;
                    }
                }
            }
            callback(true);
        })
        .Error(() => {
            callback(false);
        })
        .Execute();

        m_subRequests.Add(request);
    }

    protected abstract string taskBasePath { get; }
    protected abstract TaskTemplateType type { get; }
    
}

public class GetUserTemplateTasksRequest : GetTemplateTasksRequest
{
    public uint userId { get; set; }

    public override string path
    {
        get { return "/getprojectlist_v3.php"; }
    }

    protected override string taskBasePath
    {
        get { return "/downloadproject_v3.php"; }
    }

    protected override TaskTemplateType type
    {
        get { return TaskTemplateType.User; }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var listRes = new GetProjectListReq_V3();
        if (language == ScriptLanguage.Python) {
            listRes.ReqRoot = FileList_Root_Type.TaskTemplatePython;
        } else {
            listRes.ReqRoot = FileList_Root_Type.TaskTemplateGraphy;
        }
        request.postData = listRes.ToByteArray();
    }
}

public class GetSystemTemplateTasksRequest : GetTemplateTasksRequest
{
    public override string path
    {
        get { return "/getprojectlist_v3.php"; }
    }

    protected override string taskBasePath
    {
        get { return "/downloadproject_v3.php"; }
    }

    protected override TaskTemplateType type
    {
        get { return TaskTemplateType.System; }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var listRes = new GetProjectListReq_V3();
        if(language == ScriptLanguage.Python) {
            listRes.ReqRoot = FileList_Root_Type.TaskSystemPython;
        } else {
            listRes.ReqRoot = FileList_Root_Type.TaskSystemGraphy;
        }
        request.postData = listRes.ToByteArray();
    }
}