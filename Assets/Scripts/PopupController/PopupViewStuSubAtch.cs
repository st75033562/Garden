using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupViewStuSubAtch : PopupController {
    public ScrollLoopController scroll;

    protected override void Start () {
        List<AddAttachmentCellData> attachDatas = (List<AddAttachmentCellData>)payload;
        scroll.initWithData(attachDatas);
    }

}
