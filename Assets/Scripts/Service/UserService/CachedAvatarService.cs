using System;
using System.Collections.Generic;

public class CachedAvatarService : IAvatarService
{
    private readonly IAvatarService m_service;
    private readonly Dictionary<uint, UserAvatarInfo> m_cache = new Dictionary<uint, UserAvatarInfo>();
    private readonly Dictionary<uint, Action<UserAvatarInfo>> m_requests 
        = new Dictionary<uint, Action<UserAvatarInfo>>();

    public CachedAvatarService(IAvatarService service)
    {
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }
        m_service = service;

        UserManager.Instance.onAvatarIdChanged += OnAvatarChanged;
    }

    public void Dispose()
    {
        UserManager.Instance.onAvatarIdChanged -= OnAvatarChanged;
        m_service.Dispose();
    }

    private void OnAvatarChanged()
    {
        m_cache.Remove(UserManager.Instance.UserId);
    }

    public void GetAvatarId(uint userId, Action<UserAvatarInfo> callback)
    {
        UserAvatarInfo avatarInfo;
        if (m_cache.TryGetValue(userId, out avatarInfo))
        {
            callback(avatarInfo);
            return;
        }

        Action<UserAvatarInfo> callbacks;
        bool isRequesting = m_requests.TryGetValue(userId, out callbacks);
        callbacks += callback;
        m_requests[userId] = callbacks;

        if (!isRequesting)
        {
            m_service.GetAvatarId(userId, result => {
                m_cache.Add(userId, result);
                m_requests[userId](result);
                m_requests.Remove(userId);
            });
        }
    }
}
