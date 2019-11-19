using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CertificateNotify : MonoBehaviour {
    public Text textName;
    public Text textContent;
    public Text textTime;

    public void SetData(string nama, string content, string time) {
        textName.text = nama;
        textContent.text = content;
        textTime.text = time;
    }
}
