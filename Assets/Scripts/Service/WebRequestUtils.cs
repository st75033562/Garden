using System;
using UnityEngine;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class WebRequestBuilder
{
    private Action<ResponseData, object> m_success;
    private object m_userData;
    private Action m_failure;
    private Action m_finally;
    private string m_errorPrompt = "net_error_try_again";
    private bool m_retryOnError = true;

    public WebRequestBuilder Finally(Action always)
    {
        m_finally = always;
        return this;
    }

    public WebRequestBuilder Success(Action success)
    {
        return Success((res, obj) => success());
    }

    public WebRequestBuilder Success(Action<ResponseData, object> success)
    {
        m_success = success;
        return this;
    }

    public WebRequestBuilder DoNotRetryOnError()
    {
        m_retryOnError = false;
        return this;    
    }

    public WebRequestBuilder Fail(Action failure)
    {
        m_failure = failure;
        return this;
    }

    public WebRequestBuilder ErrorPrompt(string errorPrompty)
    {
        m_errorPrompt = errorPrompty;
        return this;
    }

    public WebRequestBuilder UserData(object userData)
    {
        m_userData = userData;
        return this;
    }

    public WebRequestData BlockingRequest()
    {
        return Request(true);
    }

    public WebRequestData Request(bool blocking = false)
    {
        int popupId = 0;
        if (blocking)
        {
            popupId = PopupManager.ShowMask();
        }
        var request = new WebRequestData();
        request.m_SuccessCallBack = (response, userData) => {
            if (blocking)
            {
                PopupManager.Close(popupId);
            }
            if (m_success != null)
            {
                m_success(response, userData);
            }

            if (m_finally != null)
            {
                m_finally();
            }
        };
        request.m_failCallBack = (data) => {
            if (m_retryOnError)
            {
                PopupManager.YesNo(m_errorPrompt.Localize(),
                    () => {
                        g_WebRequestManager.instance.AddTask(data);
                    },
                    () => {
                        if (blocking)
                        {
                            PopupManager.Close(popupId);
                        }
                        NotifyError();
                    });
            }
            else
            {
                NotifyError();
            }
        };
        request.m_Param = m_userData;
        return request;
    }

    private void NotifyError()
    {
        if (m_failure != null)
        {
            m_failure();
        }

        if (m_finally != null)
        {
            m_finally();
        }
    }
}

public static class WebRequestUtils
{
    public static void UploadAndSaveGameboard(Gameboard.Gameboard gameboard)
    {
        var request = Uploads.UploadGamboard(new GameboardProject(gameboard));
        request.Success(() => {
            GameboardRepository.instance.saveGameboard(gameboard);
        })
        .Error(() => {
            Debug.LogError("failed to upload gameboard");
        })
        .Execute();
    }
}