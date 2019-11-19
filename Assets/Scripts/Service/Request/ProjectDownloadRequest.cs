using Google.Protobuf;
using System;

public abstract class BaseProjectDownloadRequest<ResultT> : HttpRequest<ResultT>
    where ResultT : IMessage<ResultT>, new()
{
	private const string DownloadUrl = "/downloadproject_v3.php";

    public BaseProjectDownloadRequest() { }

    public BaseProjectDownloadRequest(WebRequestManager requestManager)
        : base(requestManager)
    { }

    public string basePath { get; set; }

    public bool preview { get; set; }

    public GetCatalogType type { set; get; }

    public uint userId { get; set; }

    public override string path
    {
        get { return DownloadUrl; }
    }

    protected override void Init(WebRequestData request)
    {
        if (preview)
        {
            request.Header("dl_option", "option");
        }
        if (newModel)  //新的下载模式返回FileList 对象
        {
            request.Header("dl_version2", "version2");
        }

        DownloadDirt_V3 tDownLoad = new DownloadDirt_V3();
        tDownLoad.ProjectFullpath = HttpCommon.GetRootPath(type, userId) + basePath;
        
        InitSaveAs(tDownLoad);
        request.postData = tDownLoad.ToByteArray();
    }

    protected virtual void InitSaveAs(DownloadDirt_V3 tDownLoad) {}

    protected abstract bool newModel { get; }

    protected override void OnSuccess(ResponseData response)
    {
        SetResult(ProtobufUtils.Parse<ResultT>(response.bytes));
        base.OnSuccess(response);
    }
}

public class ProjectDownloadRequest : BaseProjectDownloadRequest<FileList>
{
    public ProjectDownloadRequest() { }

    public ProjectDownloadRequest(WebRequestManager requestManager)
        : base(requestManager)
    { }

    protected override bool newModel
    {
        get { return false; }
    }
}

public class ProjectDownloadRequestV3 : BaseProjectDownloadRequest<FileList>
{
    public ProjectDownloadRequestV3() { }

    public ProjectDownloadRequestV3(WebRequestManager requestManager)
        : base(requestManager)
    { }

    protected override bool newModel
    {
        get { return true; }
    }
}

public class SingleFileDownload : HttpRequest<byte[]> {
    public string fullPath;
    public override string path {
        get {
            return fullPath;
        }
    }
    protected override void OnSuccess(ResponseData response) {
        SetResult(response.bytes);
        base.OnSuccess(response);
    }
}