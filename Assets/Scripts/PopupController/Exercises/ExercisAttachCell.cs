using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExercisAttachCellData {
    public readonly AttachData data;
    public readonly IEnumerable<AttachData> attachments;

    public ExercisAttachCellData(AttachData data, IEnumerable<AttachData> attachments) {
        Debug.Assert(data == null || attachments != null);

        this.data = data;
        this.attachments = attachments;
    }
}

public class ExercisAttachCell : BaseAttachmentCell {
    public GameObject addGo;
    public GameObject contentGo;
    public Text attachName;

    private AttachData m_attachData;
    private IEnumerable<AttachData> m_dataSource;

    public override AttachData attachData {
        get {
            return (DataObject as AddAttachmentCellData).data;
        }
    }

    public override void configureCellData() {
        bool showAdd = attachData == null;
        addGo.SetActive(showAdd);
        contentGo.SetActive(!showAdd);
        if(showAdd) {
            return;
        }

        if(attachData.resData == null) {
            attachName.text = attachData.programNickName;
        } else {
            attachName.text = attachData.resData.nickName;
        }
        initIcon();
    }

    protected override IEnumerable<AttachData> GetAttachments() {
        return (DataObject as AddAttachmentCellData).attachments;
    }

}
