using System;

public class CreateFolderRequest : UploadFileRequest
{
    public string folderPath
    {
        get;
        set;
    }

    public DateTime creationTime
    {
        get
        {
            return TimeUtils.FromEpochSeconds((long)files.GetFile(folderPath).CreateTime);
        }
    }

    protected override void Init(WebRequestData request)
    {
        files.RootPath = "";
        files.FileList_.Clear();
        AddFolder(folderPath);
        base.Init(request);
    }
}
