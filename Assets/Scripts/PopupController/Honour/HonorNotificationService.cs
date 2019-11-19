using Google.Protobuf;

public class HonorNotificationService : Singleton<HonorNotificationService>
{
    /// <summary>
    /// listen for new honor notifications
    /// </summary>
    public void Listen()
    {
        SocketManager.instance.setListener(Command_ID.CmdNewHonorNotify, OnGetNewHonors);
    }

    public void GetNewHonors()
    {
        var newHonorwallR = new CMD_Get_New_Honorwall_r_Parameters();
        SocketManager.instance.send(Command_ID.CmdGetNewHonorwallR, newHonorwallR.ToByteString(), OnGetNewHonors);
    }

    void OnGetNewHonors(Command_Result res, ByteString content)
    {
        if (res == Command_Result.CmdNoError)
        {
            var wallInfo = CMD_Get_New_Honorwall_a_Parameters.Parser.ParseFrom(content).HonorwallInfo;

            foreach (uint trophyKey in wallInfo.UserTrophies.Keys)
            {
                var trophy = UserTrophy.Parse(trophyKey, wallInfo.UserTrophies[trophyKey]);
                HonorWallData.instance.AddTrophy(trophy);
                PopupManager.TrophyNotify(trophy);
            }
            foreach (uint certificateKey in wallInfo.UserCertificates.Keys)
            {
                var certificate = UserCertificate.Parse(certificateKey, wallInfo.UserCertificates[certificateKey]);
                HonorWallData.instance.AddCertificate(certificate);
                PopupManager.CertificateNotify(certificate);
            }
        }
        else
        {
            PopupManager.Notice(res.Localize());
        }
    }
}
