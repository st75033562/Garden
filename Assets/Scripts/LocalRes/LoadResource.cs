using UnityEngine;
using System.Collections;
using System;

public class LoadResource : Singleton<LoadResource> {

    public void LoadLocalRes(string fileName, Action<WWW> action, Action onError = null)
    {
        StartCoroutine(LoadRes(fileName , action, onError));
    }

    IEnumerator LoadRes(string fileName , Action<WWW> action, Action onError)
    {
        WWW www = new WWW("file://" + fileName);
        yield return www;
        try
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                if (onError != null)
                {
                    onError();
                }
                PopupManager.Notice(www.error);
            }
            else
            {
                action(www);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            www.Dispose();
        }
    }


}
