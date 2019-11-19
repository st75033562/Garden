using System;
using Google.Protobuf;

public class PKAnswerUploadHelper
{
    private PK pk;
    private int maskId;

    public PKAnswerUploadHelper(PK pk)
    {
        if (pk == null)
        {
            throw new ArgumentNullException("pk");
        }
        this.pk = pk;
    }

    public void Upload()
    {
        if (!pk.CanUserUploadAnswer(UserManager.Instance.UserId))
        {
            PopupManager.Notice("ui_prohibit_repeated_upload".Localize());
            return;
        }
        PopupManager.ProjectView((path) => {
            string answerName = UserManager.Instance.Nickname + ":" + path.name;
            maskId = PopupManager.ShowMask();

            var code = CodeProjectRepository.instance.loadCode(path.ToString());
            CMD_Answer_PK_r_Parameters pk_answer_r = new CMD_Answer_PK_r_Parameters();
            pk_answer_r.PkId = pk.PkId;

            FileNode tCode = new FileNode();
            tCode.PathName = CodeProjectRepository.ProjectFileName;
            tCode.FileContents = ByteString.CopyFrom(code);
            pk_answer_r.PkFiles = new FileList();
            pk_answer_r.PkFiles.FileList_.Add(tCode);

            PKAnswer pkAnswer = new PKAnswer();
            pkAnswer.AnswerName = answerName;
            pk_answer_r.PkAnswerInfo = pkAnswer;

            SocketManager.instance.send(Command_ID.CmdAnswerPkR, pk_answer_r.ToByteString(), UploadBack);
        }, 
        showDeleteBtn: false, 
        showAddCell: false);
    }

    void UploadBack(Command_Result res, ByteString content)
    {
        PopupManager.Close(maskId);
        if (res == Command_Result.CmdNoError)
        {
            CMD_Answer_PK_a_Parameters pk_a = CMD_Answer_PK_a_Parameters.Parser.ParseFrom(content);
            pk.AddAnswer(pk_a.PkAnswerInfo);
        }
        else
        {
            PopupManager.Notice(res.Localize());
        }
    }
}
