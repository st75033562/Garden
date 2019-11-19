using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDAttachCell : BaseAttachmentCell {
    public Text attachName;

    private AttachData m_attachData;
    private IEnumerable<AttachData> m_dataSource;

    public override AttachData attachData {
        get {
            return DataObject as AttachData;
        }
    }

    public override void configureCellData() {
        if(attachData.resData == null) {
            attachName.text = attachData.programNickName;
        } else {
            attachName.text = attachData.resData.nickName;
        }
        initIcon();
    }

    protected override IEnumerable<AttachData> GetAttachments() {
        return null;
    }
}
