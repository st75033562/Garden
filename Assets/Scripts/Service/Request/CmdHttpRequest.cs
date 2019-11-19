using Google.Protobuf;
using System;

public struct CmdResponse<ResultT>
{
    public Command_Result errorCode;
    public ResultT result;
}

public abstract class CmdHttpRequest<RequestT, ResponseT> : HttpRequest<CmdResponse<ResponseT>>
    where RequestT : IMessage<RequestT>, new()
    where ResponseT : IMessage<ResponseT>, new()
{
    public CmdHttpRequest()
    {
        argument = new RequestT();
    }

    public CmdHttpRequest(WebRequestManager requestManager)
        : base(requestManager)
    {
        argument = new RequestT();
    }

    public abstract Command_ID cmdId { get; }

    public RequestT argument { get; set; }

    public override string path
    {
        get { return HttpCommon.c_game; }
    }

    public CmdHttpRequest<RequestT, ResponseT> Success(Action<Command_Result, ResponseT> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
        Success(x => handler(x.errorCode, x.result));
        return this;
    }

    protected override void Init(WebRequestData request)
    {
        base.Init(request);

        var cmd = new CMD();
        cmd.CmdParameters = argument.ToByteString();
        cmd.CmdId = (uint)cmdId;
        request.postData = cmd.ToByteArray();
    }

    protected override void OnSuccess(ResponseData response)
    {
        var resCmd = CMD.Parser.ParseFrom(response.bytes);
        SetResult(new CmdResponse<ResponseT> {
            errorCode = (Command_Result)resCmd.CmdResult,
            result = ProtobufUtils.Parse<ResponseT>(resCmd.CmdParameters)
        });
        base.OnSuccess(response);
    }
}
