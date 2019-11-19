using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using System;

public class ManagerBase : MonoBehaviour
{
	public UIMaskBase m_Mask;

	public virtual void RequestErrorCode(Command_Result res)
	{
		string tError = "";
		switch (res)
		{
			case Command_Result.CmdRegNameExist:
				{
					tError = "register_error".Localize("account_exists".Localize());
				}
				break;
			case Command_Result.CmdAuthError:
				{
					tError = "login_error".Localize("invalid_account".Localize());
				}
				break;
			case Command_Result.CmdUserNotFound:
				{
					tError = "login_error".Localize("ui_account_not_exist".Localize());
				}
				break;
			case Command_Result.CmdRegAlread:
				{
					tError = "account_already_registered".Localize();
				}
				break;
			default:
				{
                    tError = res.Localize();
				}
				break;
		}
		ShowMaskTips(tError);
	}

	public void ShowMask()
	{
        ShowMask("wait_for_server".Localize());
	}

    public void ShowMask(string text)
    {
        if (m_Mask)
        {
            m_Mask.ShowMask(text);
        }
    }

	public void CloseMask()
	{
		if(m_Mask)
		{
			m_Mask.CloseMask();
		}
	}

    public void ResetMask()
    {
        if (m_Mask)
        {
            m_Mask.ResetMask();
        }
    }

	public void ShowDialog(string content, Action<object> successcallback, object successparam, Action<object> failcallback = null, object failparam = null)
	{
        PopupManager.YesNo(
            content,
            () => {
                if (successcallback != null)
                {
                    successcallback(successparam);
                }
            },
            () => {
                if (failcallback != null)
                {
                    failcallback(failparam);
                }
            },
            modal: false);
	}

	public void ShowMaskTips(string content, Action<object> callback = null, object userArg = null)
	{
        PopupManager.Notice(content, () => {
            if (callback != null)
            {
                callback(userArg);
            }
        });
	}
}
