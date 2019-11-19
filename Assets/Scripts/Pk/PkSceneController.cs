using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PkSceneController : StackUIBase
{
    public StackUIBase m_multiplayerUI;

    void Awake()
    {
        StackUIBase.Push(this);
    }

    public void OnClickBack()
    {
        SceneDirector.Pop();
    }

    public void OnClickSinglePlayer()
    {
        //SceneDirector.Push("PkScene");

        //GetComponent<Canvas>().enabled = false;
        PopupManager.SinglePk(() => {
            if(GetComponent<Canvas>() != null)
                GetComponent<Canvas>().enabled = true;
        });
    }

    public void OnClickMultiPlayer()
    {
        //   Push(m_multiplayerUI);
        //GetComponent<Canvas>().enabled = false;
        PopupManager.Exercises(()=> {
            if (GetComponent<Canvas>() != null)
                GetComponent<Canvas>().enabled = true;
        });
    }

    public void OnClickCompetition()
    {
        //GetComponent<Canvas>().enabled = false;
        if (UserManager.Instance.IsAdmin)
        {
            PopupManager.AdminCompetition(() => {
                if (GetComponent<Canvas>() != null)
                    GetComponent<Canvas>().enabled = true;
            });
        }
        else
        {
            PopupManager.StudentCompetition(() => {
                if (GetComponent<Canvas>() != null)
                    GetComponent<Canvas>().enabled = true;
            });
        }
    }
}
