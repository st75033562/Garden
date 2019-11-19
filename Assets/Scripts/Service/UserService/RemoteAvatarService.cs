using Google.Protobuf;
using System;
using UnityEngine;

public class RemoteAvatarService : IAvatarService
{
    public void GetAvatarId(uint userId, Action<UserAvatarInfo> callback)
    {
        if (userId == UserManager.Instance.UserId)
        {
            callback(new UserAvatarInfo(userId,
                                        UserManager.Instance.Nickname,
                                        UserManager.Instance.AvatarID,
                                        UserManager.Instance.IsTeacher));
            return;
        }

        var request = new CMD_Get_Display_Userinfo_r_Parameters();
        request.ReqId = userId;
        SocketManager.instance.send(Command_ID.CmdGetDisplayUserinfoR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Get_Display_Userinfo_a_Parameters.Parser.ParseFrom(data);
                callback(new UserAvatarInfo(response.DispalyUserinfo.UserId,
                                            response.DispalyUserinfo.UserNickname,
                                            (int)response.DispalyUserinfo.UserInconId,
                                            ((User_Type)response.UserType & User_Type.Teacher) != 0));
            }
            else
            {
                Debug.LogError("Failed to request avatar info for user: " + userId);
            }
        });
    }

    public void Dispose() { }
}
