using Google.Protobuf;

public class GameBoardSession
{
    public void ShareGameBoard(GameboardProject project, CommandCallback callBack)
    {
        PopupManager.TwoBtnDialog("gameboard_upload_notice".Localize(),
            "gameboard_upload_yes".Localize(),
            () => ShareGameboard(project, true, callBack),
            "gameboard_upload_no".Localize(),
            () => ShareGameboard(project, false, callBack));
    }

    private void ShareGameboard(GameboardProject project, bool sourceAvailable, CommandCallback callBack)
    {
        var request = new CMD_Create_Gameboard_r_Parameters();
        request.GbName = project.gameboard.name;
        request.GbSenceId = (uint)project.gameboard.themeId;

        request.GbScriptShow = sourceAvailable ? (uint)GbScriptShowType.Show : (uint)GbScriptShowType.Hide;

        project.gameboard = new Gameboard.Gameboard(project.gameboard);
        project.gameboard.ClearCodeGroups();
        project.gameboard.sourceCodeAvailable = sourceAvailable;

        request.GbFiles = new FileList();
        request.GbFiles.FileList_.AddRange(project.ToFileNodeList(""));

        SendShareGameBoard(request, callBack);
    }

    public static void SendShareGameBoard(CMD_Create_Gameboard_r_Parameters request, CommandCallback callBack)
    {
        SocketManager.instance.send(Command_ID.CmdCreateGameboardR, request.ToByteString(), callBack);
    }
}
