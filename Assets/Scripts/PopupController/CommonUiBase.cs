using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PermitUiBase : PopupController {
    public GameObject[] hideMenu;   //空数据时需要隐藏
    public Selectable[] disableMenu;
    public GameObject[] sLJurisdiction;  //二级权限功能按钮，即老师权限
    public GameObject[] tLJurisdiction;  //三级权限功能按钮，即管理员权限
    public GameObject cancelBtn;

    protected override void Start() {
        base.Start();
        InitJurisdiction();
    }

    public bool isDisable {
        get { return cancelBtn.activeSelf; }
    }

    public void RelyOnDataMenu(bool show) {
        foreach(GameObject go in hideMenu) {
            go.SetActive(show);
        }
    }

    public void DisableOtherUi(Selectable ui) {
        if(isDisable) {
            return;
        }
        foreach(Selectable u in disableMenu) {
            u.interactable = false;
        }
        ui.interactable = true;
        cancelBtn.SetActive(true);
    }

    public void RecoverMenu() {
        foreach(Selectable u in disableMenu) {
            u.interactable = true;
        }
        cancelBtn.SetActive(false);
    }

    public virtual void InitJurisdiction() {
        if(UserManager.Instance.IsStudent) {
            foreach(GameObject go in sLJurisdiction) {
                go.SetActive(false);
            }
            foreach(GameObject go in tLJurisdiction) {
                go.SetActive(false);
            }
        } else if(UserManager.Instance.IsTeacher) {
            foreach(GameObject go in tLJurisdiction) {
                go.SetActive(false);
            }
        }
    }
}
