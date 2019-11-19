using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ClassProgramStuCell : MonoBehaviour {

    public class PayLoad {
        public string localPath;
        public uint Id;
        public string cellpath;
        public bool isLocal;
        public bool showDelete;
        public AttachData.Type attachType;
        public AttachData.State state;
        public string nickName;
        public bool relation;
    }
    public Image m_defaultImage;
    public ResourceIcons m_icons;
    public Sprite projectDefaultSprite;
    public Sprite gbDefaultSprite;

    public Text programName;
    public GameObject contentGo;
    public GameObject deleteGo;

    public PayLoad payload { get; set; }

    public void SetData(PayLoad payload) {
        this.payload = payload;
        deleteGo.SetActive(payload.showDelete);
        programName.text = payload.nickName;

        if(payload.attachType == AttachData.Type.Project) {
            m_defaultImage.sprite = projectDefaultSprite;
        } else if(payload.attachType == AttachData.Type.Gameboard) {
            m_defaultImage.sprite = gbDefaultSprite;
        }
    }

}
