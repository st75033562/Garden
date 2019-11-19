using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteRequest : HttpRequest<byte[]> {
    public string basePath;

    public GetCatalogType type { set; get; }

    public uint userId { get; set; }

    public override string path {
        get {
            return "/delproject_v3.php";
        }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var delV3 = new DownloadDirt_V3();
        delV3.ProjectFullpath = HttpCommon.GetRootPath(type, userId) + basePath;

        request.postData = delV3.ToByteArray();

    }

    protected override void OnSuccess(ResponseData response) {
        SetResult(response.bytes);
        base.OnSuccess(response);
    }
}
