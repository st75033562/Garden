using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPasswordData {
    public Action<string> valueCallBack;
    public string password;
    public SetPasswordData(Action<string> valueCallBack , string password = null) {
        this.valueCallBack = valueCallBack;
        this.password = password;
    }

}
public class PopupSetPassword : PopupController {
    [SerializeField]
    private InputField inputField;
    [SerializeField]
    private GameObject[] marks;
    [SerializeField]
    private Text[] textChars;
    [SerializeField]
    private Button btnOk;


    private SetPasswordData setPasswordData;
    // Use this for initialization
    protected override void Start () {
       base.Start();

        inputField.ActivateInputField();

        setPasswordData = (SetPasswordData)payload;
        if(setPasswordData.password != null) {
            inputField.interactable = false;
            for(int i=0; i< textChars.Length; i++) {
                textChars[i].gameObject.SetActive(true);
                textChars[i].text = setPasswordData.password.Substring(i ,1);
            }
        }

    }

    public void OnClickOk() {
        if(setPasswordData.password == null)
            setPasswordData.valueCallBack(inputField.text);
        OnCloseButton();
    }

    public void ValueChange(string str) {
        for(int i=0; i< marks.Length; i++) {
            if(i < str.Length) 
                marks[i].SetActive(true);
            else
                marks[i].SetActive(false);
        }
        btnOk.interactable = str.Length == marks.Length;
    }
}
