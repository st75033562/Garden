using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupGameBoardBank : PermitUiBase
{
    public class SelectData {
        public string path;
        public string password;
        public List<string> catalogs = new List<string>();
    }

    public GameObject[] contentPanels;

    public GBBankShareBase[] gbBankShareBase;
    public GameObject confirmGo;
    public GameObject deleteGO;
    public GameObject downloadGO;
    public GameObject addGO;

    protected override void Start()
    {
        deleteGO.SetActive(UserManager.Instance.IsAdmin);
        addGO.SetActive(UserManager.Instance.IsAdmin);
        Action<SelectData> selectBack = payload as Action<SelectData>;
        if (selectBack != null)
        {
            confirmGo.SetActive(true);
            deleteGO.SetActive(false);
            downloadGO.SetActive(false);
            foreach (var gb in gbBankShareBase)
            {
                gb.selectPathBack = (data) => {
                    Close();
                    confirmGo.SetActive(false);
                    selectBack(data);
                };
            }
        }
        base.Start();
    }

    public void SwitchPanel(GameObject go)
    {
        foreach (GameObject panel in contentPanels)//需要先执行 ondisable 
        {
            panel.SetActive(false);
        }
        go.SetActive(true);
    }

    public void BackOrClose() {
        foreach (var gb in gbBankShareBase)
        {
            if (gb.gameObject.activeSelf && gb.Close()) {
                Close();
            }
        }
    }
}
