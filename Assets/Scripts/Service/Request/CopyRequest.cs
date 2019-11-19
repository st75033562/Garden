using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRequest : HttpRequest<byte[]> {

    public FileList_Root_Type rootType;
    public string projectSrc;
    public string desName;
    public string desTag;

    public GetCatalogType type { set; get; }

    public uint userId { get; set; }

    public override string path {
        get {
            return "/copyproject_v3.php";
        }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var copyV3 = new CopyDir_V3();
        copyV3.ProjectSrc = projectSrc;
        copyV3.DesRoot = rootType;
        if(!string.IsNullOrEmpty(desTag)) {
            copyV3.DesTag = desTag;
        }        
        copyV3.DesName = desName;

        request.postData = copyV3.ToByteArray();

    }

    protected override void OnSuccess(ResponseData response) {
        SetResult(response.bytes);
        base.OnSuccess(response);
    }
}
