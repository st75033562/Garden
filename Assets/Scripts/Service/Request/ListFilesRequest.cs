using Google.Protobuf;
using System;

public class ListFilesRequest : HttpRequest<FileList>
{
    public ListFilesRequest()
    {
    }

    public ListFilesRequest(WebRequestManager requestManager)
        : base(requestManager)
    {
    }

    public override string path
    {
        get
        {
            return "/getprojectlist_v3.php";
        }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var listRes = new GetProjectListReq_V3();

        switch(type) {
            case GetCatalogType.TEACHER_TASK:
                listRes.ReqRoot = FileList_Root_Type.TaskTemplateGraphy;
                break;

            case GetCatalogType.SYSTEM_TASK:
                listRes.ReqRoot = FileList_Root_Type.TaskSystemGraphy;
                break;

            case GetCatalogType.PYTHON:
                listRes.ReqRoot = FileList_Root_Type.SelfPython;
                break;
            case GetCatalogType.SELF_PROJECT_V2:
                listRes.ReqRoot = FileList_Root_Type.SelfGraphy;
                break;

            case GetCatalogType.GAME_BOARD_V2:
                listRes.ReqRoot = FileList_Root_Type.SelfGbGraphy;
                break;

            default:
                throw new ArgumentOutOfRangeException("type");
        }

        request.postData = listRes.ToByteArray();
    }

    public GetCatalogType type { get; set; }

    protected override void OnSuccess(ResponseData response)
    {
        var multiFiles = ProtobufUtils.Parse<MultiFileList>(response.bytes);
        foreach (var fileList in multiFiles.FileLists.Values)
        {
            SetResult(fileList);
        }
        base.OnSuccess(response);
    }
}
