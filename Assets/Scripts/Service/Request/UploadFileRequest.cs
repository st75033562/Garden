using Google.Protobuf;
using System;

public class UploadFileRequest : HttpRequest
{
    private FileList m_files = new FileList();
    private GetCatalogType m_type = GetCatalogType.SELF_PROJECT_V2;

    public UploadFileRequest()
    { }

    public UploadFileRequest(WebRequestManager requestManager)
        : base(requestManager)
    {
    }

    public FileList files { get { return m_files; } }

    public void AddFile(string path, byte[] data)
    {
        var fileNode = new FileNode();
        fileNode.PathName = path;
        fileNode.FileContents = ByteString.CopyFrom(data);
        fileNode.FnType = (uint)FN_TYPE.FnFile;
        files.FileList_.Add(fileNode);
    }

    public void AddFolder(string path)
    {
        var fileNode = new FileNode();
        fileNode.PathName = path;
        fileNode.FnType = (uint)FN_TYPE.FnDir;
        files.FileList_.Add(fileNode);
    }

    public GetCatalogType type
    {
        get { return m_type; }
        set
        {
            if (value < GetCatalogType.PYTHON || value > GetCatalogType.GAME_BOARD_V2)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            m_type = value;
        }
    }

    public override string path
    {
        get
        {
            return "/uploadproject_v3.php";
        }
    }

    protected override void Init(WebRequestData request)
    {
        base.Init(request);
        var uploadFileV3 = new UploadFileList_V3();
        uploadFileV3.ListRoot = FileList_Root_Type.ClassProjectRoot;

        switch(type) {
            case GetCatalogType.PYTHON:
                uploadFileV3.ListRoot = FileList_Root_Type.SelfPython;
                break;
            case GetCatalogType.SELF_PROJECT_V2:
                uploadFileV3.ListRoot = FileList_Root_Type.SelfGraphy;
                break;
            case GetCatalogType.GAME_BOARD_V2:
                uploadFileV3.ListRoot = FileList_Root_Type.SelfGbGraphy;
                break;
        }
        uploadFileV3.Files = files;

        request.postData = uploadFileV3.ToByteArray();
    }

    protected override void OnSuccess(ResponseData response) {
        var uploadFileV3 = UploadFileList_V3.Parser.ParseFrom(response.bytes);
        m_files = uploadFileV3.Files;
        base.OnSuccess(response);
    }
}
