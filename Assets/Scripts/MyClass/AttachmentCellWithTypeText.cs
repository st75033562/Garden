using UnityEngine;
using UnityEngine.UI;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class AttachmentCellWithTypeText : MonoBehaviour
{
    public Image m_defaultImage;
    public ResourceIcons m_icons;
    public Text m_nameText;
    public Sprite projectDefaultSprite;
    public Sprite gbDefaultSprite;
    public TaskCellDetail taskCellDetail;

    private LocalResData localResData;
    private TaskEnclosureCell cellData;
    public void SetData(TaskEnclosureCell cellData)
    {
        this.cellData = cellData;
        if(cellData.attachType == AttachData.Type.Project) {
            m_nameText.text = cellData.programName;
            m_defaultImage.sprite = projectDefaultSprite;
        }else if(cellData.attachType == AttachData.Type.Gameboard) {
            m_nameText.text = cellData.programName;
            m_defaultImage.sprite = gbDefaultSprite;
        } else {
            localResData = cellData.localResData;
            m_nameText.text = localResData.nickName;
            m_defaultImage.overrideSprite = m_icons.GetIcon(localResData.resType);
        }
    }

    public void OnClick() {
        if(localResData == null) {
             taskCellDetail.OnClickDown(cellData.projectId, cellData.programName, cellData.attachType);
        } else {
            AttachInfo attachInfo = new AttachInfo();
            attachInfo.resType = AttachInfo.ResType.Resource;
            attachInfo.resData = localResData;
            AttachmentPreview.Preview(attachInfo);
        }
    }
}