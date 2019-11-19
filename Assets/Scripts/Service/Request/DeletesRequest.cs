using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletesRequest : HttpRequest<byte[]> {

    public List<string> fullPaths = new List<string>();

    public override string path {
        get {
            return "/delproject_v3.php";
        }
    }

    protected override void Init(WebRequestData request) {
        base.Init(request);
        var delV3 = new DelDir_V3();
        delV3.DelPaths.AddRange(fullPaths);
        request.postData = delV3.ToByteArray();

    }

    protected override void OnSuccess(ResponseData response) {
        SetResult(response.bytes);
        base.OnSuccess(response);
    }
}
