using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkSessionController : Singleton<NetworkSessionController>, IHttpAuthErrorHandler, IHttpTaskErrorDefaultHandler
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<NetworkSessionController>();

    public Action onBeforeLogout;
    public Action onLoggedOut;

    private Action m_authErrorResolved;
    private readonly List<WebRequestData> m_failedTasks = new List<WebRequestData>();

    private int m_connectionMaskId;
    private int m_failedTaskPopupId;
    private WebRequestData m_handlingFailedTask;
    private int m_maskRefCount;
    private int m_blockingRequestCount;
    private bool m_reconnecting;

    private const int AuthErrorResolutionTimeout = 15;

    public void Initialize(SocketManager socketManager, WebRequestManager webRequestManager)
    {
        this.socketManager = socketManager;
        this.socketManager.onLoggedIn += OnLoggedIn;
        this.socketManager.onReconnectStateChanged += OnReconnectionStateChanged;
        this.socketManager.onDisconnected += OnDisconnected;

        this.webRequestManager = webRequestManager;
        this.webRequestManager.authErrorHandler = this;
        this.webRequestManager.taskErrorDefaultHandler = this;
        this.webRequestManager.beforeRequest += OnBeforeRequest;
        this.webRequestManager.onRequestComplete += OnRequestComplete;

        SceneDirector.onLoadingStateChanged += OnLoadingStateChanged;
    }

    private void ShowMask()
    {
        if (m_maskRefCount++ == 0)
        {
            m_connectionMaskId = PopupManager.ShowMask();
        }
    }

    private void HideMask()
    {
        if (m_maskRefCount > 0 && --m_maskRefCount == 0)
        {
            PopupManager.Close(m_connectionMaskId);
        }
        else if (m_maskRefCount == 0)
        {
            s_logger.LogWarning("connection mask already closed");
        }
    }

    private void OnRequestComplete(WebRequestData request)
    {
        if (request.blocking)
        {
            --m_blockingRequestCount;
            HideMask();
        }
    }

    private void OnBeforeRequest(WebRequestData request)
    {
        if (UserManager.Instance.UserId != 0)
        {
            request.Header("user_id", UserManager.Instance.UserId.ToString());
        }
        if (UserManager.Instance.Token != null)
        {
            // role_token or account_token is used in legacy requests
            request.Header("role_token", UserManager.Instance.Token);
            request.Header("account_token", UserManager.Instance.Token);
            request.Header("user_token", UserManager.Instance.Token);
        }

        if (request.blocking)
        {
            ++m_blockingRequestCount;
            ShowMask();
        }
    }

    private void OnLoggedIn()
    {
        s_logger.Log(socketManager.token);
    }

    private void OnLoadingStateChanged()
    {
        socketManager.processingRequests = !SceneDirector.IsLoading;
    }

    public SocketManager socketManager { get; private set; }

    public WebRequestManager webRequestManager { get; private set; }

    private void OnDisconnected(DisconnectReason reason)
    {
        if (reason == DisconnectReason.SessionExpired)
        {
            PopupManager.Notice("auth_verification_fail".Localize(), Logout, queued: false);
        }
        else
        {
            s_logger.Log("disconnected " + reason);
        }
    }

    public void Logout()
    {
        Logout(true);
    }

    public void Logout(bool gotoLobby)
    {
        if (onBeforeLogout != null)
        {
            onBeforeLogout();
        }

        m_blockingRequestCount = 0;
        m_handlingFailedTask = null;
        m_failedTasks.Clear();
        PopupManager.Close(m_failedTaskPopupId);

        CloseConnectionMask();

        m_authErrorResolved = null;
        StopAllCoroutines();

        m_reconnecting = false;
        socketManager.disconnect();
        webRequestManager.Reset();

        if (gotoLobby)
        {
            Utils.GotoHomeScene();
        }

        if (onLoggedOut != null)
        {
            onLoggedOut();
        }
    }

    private void CloseConnectionMask()
    {
        m_maskRefCount = 0;
        PopupManager.Close(m_connectionMaskId);
    }

    private void OnReconnectionStateChanged(ReconnectState state)
    {
        if (state == ReconnectState.Reconnecting)
        {
            s_logger.Log("reconnecting");

            if (!m_reconnecting)
            {
                m_reconnecting = true;
                ShowMask();
            }

            // temporarily hide the retry dialog
            if (m_handlingFailedTask != null)
            {
                m_failedTasks.Insert(0, m_handlingFailedTask);
                m_handlingFailedTask = null;
                PopupManager.Close(m_failedTaskPopupId);
            }
        }
        else if (state == ReconnectState.ReconnectFailed)
        {
            s_logger.Log("reconnection failed");

            PopupManager.YesNo("net_error_try_again".Localize(),
            () => {
                socketManager.reconnect();
            },
            Logout,
            queued: false);
        }
        else
        {
            s_logger.Log("reconnected");

            m_reconnecting = false;
            HideMask();

            UserManager.Instance.Token = socketManager.token;
            StopAllCoroutines();
            NotifyAuthErrorResolved();
        }
    }

    void NotifyAuthErrorResolved()
    {
        if (m_authErrorResolved != null)
        {
            m_authErrorResolved();
            m_authErrorResolved = null;
        }
    }

    void IHttpAuthErrorHandler.Handle(Action resolved)
    {
        s_logger.LogError("http unauthorized");

        m_authErrorResolved = resolved;

        StopAllCoroutines();
        // reconnecting, no need to start timer
        if (!m_reconnecting)
        {
            StartCoroutine(ResolveAuthErrorTimer());
        }
    }

    IEnumerator ResolveAuthErrorTimer()
    {
        yield return new WaitForSecondsRealtime(AuthErrorResolutionTimeout);

        s_logger.Log("auth error resolution timed out, connection state {0}", socketManager.state);

        if (socketManager.state == SocketManager.State.LoggedIn)
        {
            s_logger.Log("user is logged in, assume auth error has been resolved");

            NotifyAuthErrorResolved();
        }
        else if (!m_reconnecting)
        {
            s_logger.LogError("not reconnecting, could not resolve auth error");

            // TODO: try to login again?
            PopupManager.Notice("ui_network_reconnection_unknown_error".Localize(), Logout, queued: false);
        }
        else
        {
            s_logger.Log("wait for reconnection to complete");
        }
    }

    void IHttpTaskErrorDefaultHandler.Handle(WebRequestData task)
    {
        m_failedTasks.Add(task);
    }

    void Update()
    {
        if (m_authErrorResolved == null &&
            m_failedTasks.Count > 0 && m_handlingFailedTask == null &&
            m_blockingRequestCount == 0 && 
            !m_reconnecting)
        {
            m_handlingFailedTask = m_failedTasks[0];
            m_failedTasks.RemoveAt(0);

            var errorPrompt = m_handlingFailedTask.defaultErrorPrompt;
            if (string.IsNullOrEmpty(errorPrompt))
            {
                errorPrompt = "net_error_try_again".Localize();
            }
            m_failedTaskPopupId = PopupManager.YesNo(
                errorPrompt,
                () => {
                    webRequestManager.AddTask(m_handlingFailedTask);
                    m_handlingFailedTask = null;
                },
                () => {
                    webRequestManager.NotifyRequestFail(m_handlingFailedTask);
                    m_handlingFailedTask = null;
                });
        }
    }
}
