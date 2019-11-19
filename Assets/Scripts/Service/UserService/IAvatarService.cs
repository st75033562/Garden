using System;

public class UserAvatarInfo
{
    public UserAvatarInfo(uint userId, string nickname, int avatarId, bool isTeacher)
    {
        this.userId = userId;
        this.avatarId = avatarId;
        this.nickname = nickname;
        this.isTeacher = isTeacher;
    }

    public uint userId { get; private set; }

    public bool isTeacher { get; private set; }

    public int avatarId { get; private set; }

    public string nickname { get; private set; }
}

public interface IAvatarService : IDisposable
{
    void GetAvatarId(uint userId, Action<UserAvatarInfo> callback);
}
