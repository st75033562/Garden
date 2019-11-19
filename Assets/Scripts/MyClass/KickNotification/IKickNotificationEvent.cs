using System;

public interface IKickNotificationEvent
{
    event Action<KickNotification> onDidConfirmNotification;
    event Action onCompleteNotifications;
}
