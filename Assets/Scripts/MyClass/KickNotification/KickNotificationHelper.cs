using System;
using System.Collections;
using UnityEngine;

public class KickNotificationHelper : MonoBehaviour, IKickNotificationEvent
{
    public event Action<KickNotification> onDidConfirmNotification;
    public event Action onCompleteNotifications;

    private bool m_showingNotification;

    void Update()
    {
        if (UserManager.Instance.KickNotifications.Count > 0)
        {
            if (!m_showingNotification)
            {
                m_showingNotification = true;
                StartCoroutine(ShowNextNotificationImpl(UserManager.Instance.KickNotifications[0]));
            }
        }
    }

    IEnumerator ShowNextNotificationImpl(KickNotification notification)
    {
        while (SocketManager.instance.hasOutgoingRequests ||
            Singleton<WebRequestManager>.instance.hasRequests)
        {
            yield return null;
        }

        string tip = "ui_kick_notification".Localize(notification.className);
        PopupManager.Notice(tip, delegate {
            // non-blocking
            UserManager.Instance.RemoveKickNotification(notification.id);
            NetManager.instance.RemoveKickNotifications(new KickNotification[] { notification }, null);

            if (onDidConfirmNotification != null)
            {
                onDidConfirmNotification(notification);
            }

            if (UserManager.Instance.KickNotifications.Count == 0 && onCompleteNotifications != null)
            {
                onCompleteNotifications();
            }

            m_showingNotification = false;
        });
    }
}
