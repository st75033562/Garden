using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSelectCertificate : PopupController {
    public ScrollLoopController scroll;

    private Action<int> selectId;
    private SelectCertificateCell selectCell;
    // Use this for initialization
    protected override void Start () {
        base.Start();
        selectId = (Action<int>)payload;

        List<CertificateData> certicateDatas = CertificateData.GetAllCerticateData();

        scroll.initWithData(certicateDatas);
    }

    public void OnClickCell(SelectCertificateCell cell) {
        if(selectCell != null) {
            selectCell.SwitchBtn(true);
        }
        selectCell = cell;
        selectCell.SwitchBtn(false);
    }

    public void OnClickConfirm() {
        if(selectCell != null) {
            selectId.Invoke(selectCell.certificateData.id);
        }
        OnCloseButton();
    }
}
