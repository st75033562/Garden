using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class GradeMark
{
    public const int MinGrade = 1;
    public const int MaxGrade = 6;

    public static bool IsValid(int grade)
    {
        return MinGrade <= grade && grade <= MaxGrade;
    }

    public static string GetString(int grade)
    {
        if (!IsValid(grade))
        {
            throw new ArgumentOutOfRangeException("grade");
        }
        return ((char)((int)'A' + grade - 1)).ToString();
    }
}

// NOTE: it's temporary to make WebRequestManager partial so that we don't need to change client code.
// Ideally, each request should belong to a corresponding service instead of being added to WebRequestManager.

public partial class WebRequestManager
{
    const string c_TaskUploadPhpV3 = "/uploadproject_v3.php";

    const string c_TaskUploadPhp = "/uploadtask_pro.php";
	const string c_delProject = "/delproject.php";
	const string c_UpLoadCommentPhp = "/addtaskcomm.php";
	const string c_AddsystempToSelf = "/addsystemplate2self.php";
	const string c_publishtaskfromtemplate = "/publishtaskfromtemplate_v3.php";
	public const string c_CommentFileName = "comm";
    const string c_downloadmedia = "/download/media/";

    // necessary for server to identify the video media
    const string c_VideoExtension = ".mp4";
    const string c_VideoShareUrl = "/playvideo.php?video_id=";

    UploadFileList_V3 PackProjectToBuff(string projectPath, string path, string projectTag)
    {
        var project = CodeProjectRepository.instance.loadCodeProject(projectPath);

        UploadFileList_V3 tProject = new UploadFileList_V3();
        PackProjectToBuff(tProject, project, path, projectTag);
        return tProject;
    }

    void PackProjectToBuff(UploadFileList_V3 tProject, Project project, string path, string projectTag)
    {
        if (projectTag != null)
        {
            tProject.ProjectTag = projectTag;
        }
        FileList fileList = new FileList();
        fileList.FileList_.AddRange(project.ToFileNodeList(path));
        tProject.Files = fileList;
    }

    public int UploadTaskProject(string projectPath, string classID, string taskID, WebRequestData request)
    {
        request.url = UrlHost + c_TaskUploadPhpV3;
        var uploadV3 = PackProjectToBuff(projectPath, "", null);
        uploadV3.ListRoot = FileList_Root_Type.ClassProjectRoot;

        request.postData = uploadV3.ToByteArray();
        request.Header("class_id", classID);
        request.Header("task_id", taskID);

        return AddTask(request);
    }

    public int UploadTaskProject(FileCollection pythonFiles, string classID, string taskID, WebRequestData request) {
        request.url = UrlHost + c_TaskUploadPhpV3;
        UploadFileList_V3 uploadV3 = new UploadFileList_V3();
        uploadV3.Files = new FileList();
        uploadV3.Files.FileList_.AddRange(pythonFiles.ToFileNodeList(""));
        uploadV3.ListRoot = FileList_Root_Type.ClassProjectRoot;

        request.postData = uploadV3.ToByteArray();
        request.Header("class_id", classID);
        request.Header("task_id", taskID);

        return AddTask(request);
    }

    public int UploadTaskProject(Project project, string classID, string taskID, WebRequestData request) {
        UploadFileList_V3 uploadV3 = new UploadFileList_V3();
        PackProjectToBuff(uploadV3, project, UserManager.Instance.UserId.ToString() + "/", null);

        uploadV3.ListRoot = FileList_Root_Type.ClassProjectRoot;

        request.url = UrlHost + c_TaskUploadPhpV3;
        request.postData = uploadV3.ToByteArray();
        request.Header("class_id", classID);
        request.Header("task_id", taskID);

        return AddTask(request);
    }

    public void addsysTemplateToself(string base64Path, WebRequestData request)
    {
        request.url = UrlHost + c_AddsystempToSelf;
        request.Header("project_src_path", base64Path);
        request.Header("project_des_path", base64Path);

        AddTask(request);
    }
    public void PublishTaskFromPools(string base64Path, uint classId, WebRequestData request)
    {
        request.url = UrlHost + c_publishtaskfromtemplate;
        request.Header("project_path", base64Path);
        request.Header("class_id", classId.ToString());

        AddTask(request);
    }

    public static string GetProjectPath(uint courseId, uint periodId, uint itemId)
    {
        return "/download/course_v3/" + courseId + "/" + periodId + "/" + itemId;
    }

    public int UpLoadComment(string taskID, string classID, string submitID, Project project, WebRequestData request)
    {
        request.url = UrlHost + c_TaskUploadPhpV3;
        request.Header("task_id", taskID);
        request.Header("class_id", classID);
        request.Header("submit_id", submitID);

        UploadFileList_V3 uploadV3 = new UploadFileList_V3();
        PackProjectToBuff(uploadV3, project, "", null);
        uploadV3.ListRoot = FileList_Root_Type.TaskComm;

        request.postData = uploadV3.ToByteArray();

        return AddTask(request);
    }

    public string GetMediaPath(string name, bool isVideo)
    {
        return UrlHost + c_downloadmedia + GetMediaFilename(name, isVideo);
    }

    private static string GetMediaFilename(string name, bool isVideo)
    {
        return isVideo ? name + c_VideoExtension : name;
    }

    public string GetVideoShareUrl(string videoName)
    {
        return UrlHost + c_VideoShareUrl + GetMediaFilename(videoName, true);
    }
}
