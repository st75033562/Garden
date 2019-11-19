using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

public class UploadTemplateTaskRequest : HttpRequest<string>
{
	private const string TeacherTaskPoolPath = "/uploadtasktemplatefile.php";
	private const string SysTaskPoolPath = "/uploadsystasktemplatefile.php";
    private ScriptLanguage m_language = ScriptLanguage.Num;

    public TaskTemplateType type { get; set; }
    public string taskName { get; set; }
    public string taskDescription { get; set; }
    public DateTime creationTime { get; set; }
    public K8_Attach_Info attachs { get; set; }
    public TaskCategory level { get; set; }

 //   public string projectName { get; set; }

    public FileCollection pythonFiles { get; set; }

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

    public override string path
    {
        get
        {
            return "/uploadproject_v3.php";
        }
    }

    protected override void Validate()
    {
        if (language == ScriptLanguage.Num)
        {
            throw new InvalidRequestException("language");
        }
    }

    protected override void Init(WebRequestData request)
    {
        var projectFiles = new UploadFileList_V3();
        projectFiles.Files = new FileList();

        var gbAttach = attachs.AttachList.Values.ToList().Find(x => { return x != null && x.AttachType == K8_Attach_Type.KatGameboard; });
        if(gbAttach != null && gbAttach.AttachFiles != null) {
            Gameboard.Gameboard gameboard = gbAttach.AttachFiles.GetGameboard(); ;
            var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
            groups.ClearCodeGroups();
            int i = 0;
            foreach(uint key in attachs.AttachList.Keys) {
                if(attachs.AttachList[key].AttachType == K8_Attach_Type.KatProjects && !string.IsNullOrEmpty(attachs.AttachList[key].ClientData)) {
                    var remotePath = Gameboard.ProjectUrl.ToRemote(key.ToString());
                    var group = new Gameboard.RobotCodeGroupInfo(remotePath);
                    group.Add(i++);
                    group.projectName = attachs.AttachList[key].AttachName;
                    groups.Add(group);
                }
            }
            gbAttach.AttachFiles.GetFile(GameboardRepository.GameBoardFileName).FileContents = gameboard.Serialize().ToByteString();
        }

        foreach (uint key in attachs.AttachList.Keys)
        {
            var attachUnit = attachs.AttachList[key];
            
            
            if(attachUnit.AttachFiles != null) {
                foreach (FileNode node in attachUnit.AttachFiles.FileList_)
                {
                    node.PathName = taskName + "/" + key + "/" + node.PathName;
                    projectFiles.Files.FileList_.Add(node);
                }
            }
        }
        projectFiles.ProjectTag = ((int)level).ToString();

        var taskInfo = new A8_Task_Info();
        taskInfo.TaskName = taskName;
        taskInfo.TaskDescription = taskDescription;
        taskInfo.TaskProgramName = taskName;
        taskInfo.TaskCreateTime = (ulong)creationTime.SecondsSinceEpoch();

        taskInfo.TaskAttachInfoNew = attachs;

        var fileNode = new FileNode();
        fileNode.PathName = taskName + "/" + TaskCommon.TaskFileName;
        fileNode.FileContents = taskInfo.ToByteString();
        projectFiles.Files.FileList_.Add(fileNode);

        if(type == TaskTemplateType.User && language == ScriptLanguage.Visual) {
            projectFiles.ListRoot = FileList_Root_Type.TaskTemplateGraphy;
        }else if(type == TaskTemplateType.User && language == ScriptLanguage.Python) {
            projectFiles.ListRoot = FileList_Root_Type.TaskTemplatePython;
        } else if(type == TaskTemplateType.System && language == ScriptLanguage.Visual) {
            projectFiles.ListRoot = FileList_Root_Type.TaskSystemGraphy;
        } else {
            projectFiles.ListRoot = FileList_Root_Type.TaskSystemPython;
        }

        request.postData = projectFiles.ToByteArray();
    }

    protected override void OnSuccess(ResponseData response)
    {
        var path = RequestUtils.Base64Encode(((int)level).ToString()) + "/" + RequestUtils.Base64Encode(taskName);
        SetResult(path);

        base.OnSuccess(response);
    }
}
