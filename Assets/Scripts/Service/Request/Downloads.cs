using System;

public static class Downloads
{
    public static ProjectDownloadRequest DownloadComment(uint classId, uint taskId, uint userId)
    {
        var request = new ProjectDownloadRequest();
        request.basePath = TaskCommon.GetTaskPath(classId, taskId, userId)
            + "/" + RequestUtils.Base64Encode(WebRequestManager.c_CommentFileName);

        request.type = GetCatalogType.EMPTY;
        return request;
    }

    public static ProjectDownloadRequest DownloadTask(uint classId, uint taskId, uint userId)
    {
        var request = new ProjectDownloadRequest();
        request.basePath = TaskCommon.GetTaskPath(classId, taskId, userId);
        return request;
    }

    public static SaveProjectAsRequest SaveTaskAs(uint classId, uint taskId, uint userId, string projectId, string saveAs, bool isGb = false)
    {
        var request = new SaveProjectAsRequest();
        request.basePath = TaskCommon.GetTaskPath(classId, taskId, userId) + "/"+ projectId;
        if(isGb) {
            request.saveAsType = CloudSaveAsType.GameBoard;
        } else if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            request.saveAsType = CloudSaveAsType.Project;
        } else {
            request.saveAsType = CloudSaveAsType.ProjectPy;
        }
        request.type = GetCatalogType.EMPTY;
        request.saveAs = saveAs;
        return request;
    }
    
    public static SingleFileDownload Download(GetCatalogType type, string projectId, uint userId)
    {
        var request = new SingleFileDownload();
        request.fullPath = projectId;
        return request;
    }

    public static SimpleHttpRequest DownloadVoice(uint userID, string voiceName)
    {
        if (string.IsNullOrEmpty(voiceName))
        {
            throw new ArgumentException("voiceName");
        }

        var request = new SimpleHttpRequest();
        request.SetPath("/" + HttpCommon.c_VoicePath + userID.ToString() + "/" + voiceName);
        return request;
    }

    public static SimpleHttpRequest DownloadMedia(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name");
        }

        var request = new SimpleHttpRequest();
        request.SetPath(HttpCommon.c_downloadmedia + name);
        return request;
    }
}
