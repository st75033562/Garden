using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClassShowInfo : ScrollCell {
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Text className;
    private MyClassController classController;
    private ClassInfo banjiInfo;
	
    public override void configureCellData ()
    {
        ClassShowInfoData data = (ClassShowInfoData)DataObject;
        if(data == null)
            return;
        classController = data.classController;
        banjiInfo = data.banji;
        
        icon.sprite = ClassIconResource.GetIcon((int)banjiInfo.m_IconID);
        className.text = banjiInfo.m_Name;
    }

    public void OnClick () {
        classController.OnItemClick (banjiInfo);
    }
}
