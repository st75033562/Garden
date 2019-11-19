using System;
using UnityEngine;

public class TestKickNotificationService : MonoBehaviour, IKickNotificationEvent
{
    public event Action<KickNotification> onDidConfirmNotification;

    public event Action onCompleteNotifications;

    private uint m_classId;
    private string m_className;

    public void SetCurrentClass(uint classId, string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentException("className");
        }

        m_classId = classId;
        m_className = className;
    }
        
    [ContextMenu("Fire Confirm")]
    public void FireConfirm()
    {
        PopupManager.Notice("ui_kick_notification".Localize(m_className), delegate {
            if (onDidConfirmNotification != null)
            {
                onDidConfirmNotification(new KickNotification {
                    classId = m_classId,
                    className = m_className
                });
            }
        });
    }

    [ContextMenu("Fire Complete")]
    public void FireComplete()
    {
        if (onCompleteNotifications != null)
        {
            onCompleteNotifications();
        }
    }
}
