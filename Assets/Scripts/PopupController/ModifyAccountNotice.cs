using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifyAccountNotice : MonoBehaviour {
    public Text contentText;

    private Action confirmCallBack;
    public void SetData(string content, Action confirmCallBack) {
        this.confirmCallBack = confirmCallBack;
        contentText.text = content;
    }

    public void OnClickConfirm() {
        confirmCallBack();
    }
}
