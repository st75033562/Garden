using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttachmentInfoCell : BaseAttachmentCell {
    public InputField inputName;
    public UIButtonToggle codeToggle;
    public GameObject delGo;
    public PopupAttachmentManager attachmentManager;

    void Start() {
        if(codeToggle) {
            codeToggle.beforeChangingState += OnBeforeToggleCode;
        }
    }

    public override AttachData attachData
    {
        get { return (AttachData)DataObject; }
    }

    public override void configureCellData() {
        if(DataObject == null) {
            gameObject.SetActive(false);
            return;
        }

        initIcon();
        if(attachData.resData == null) {
            inputName.text = attachData.programNickName;
        } else {
            inputName.text = attachData.resData.nickName;
        }
        delGo.SetActive(!attachData.hideDelReal);
        codeToggle.gameObject.SetActive(attachmentManager.robotCount != 0 && attachData.type == AttachData.Type.Project && !attachData.hideDelReal);
        if(attachmentManager.GameboardCount > 1) {
            codeToggle.isOn = false;
            attachData.isRelation = false;
            codeToggle.gameObject.SetActive(false);
        }
        if(codeToggle.gameObject.activeSelf) {
            codeToggle.isOn = attachData.isRelation;
        }
    }

    public void InputNickName() {
        if(inputName != null) {
            if(attachData.resData == null) {
                attachData.programNickName = inputName.text;
            } else {
                attachData.resData.nickName = inputName.text;
            }
        }
    }

    bool OnBeforeToggleCode(bool isOn) {
        return attachmentManager.OnBeforeToggleCodeBinding(attachData, isOn);
    }

    protected override IEnumerable<AttachData> GetAttachments()
    {
        return attachmentManager.GetAttachments();
    }
}
