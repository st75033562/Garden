using UnityEngine;
using UnityEngine.UI;

public class AttachmentCellBase : ScrollableCell
{
    public Image m_defaultImage;
    public ResourceIcons m_icons;

    public override void ConfigureCellData()
    {
        var localResData = resourceData;

        m_defaultImage.overrideSprite = m_icons.GetIcon(localResData.resType);
    }

    protected virtual LocalResData resourceData
    {
        get { return DataObject as LocalResData; }
    }

    private void OnImageLoaded(UIImageMedia image)
    {
        m_defaultImage.enabled = false;
    }

    public virtual void OnClick()
    {
        var localResData = resourceData;
        if(Utils.IsValidUrl(localResData.name)) {
            Application.OpenURL(localResData.name);
            return;
        }

        if(localResData.resType == ResType.Image) {
            if(localResData.textureData != null) {
                PopupManager.ImagePreview(localResData.textureData);
            } else {
                PopupManager.ImagePreview(localResData.name);
            }
        } else if(localResData.resType == ResType.Video) {
            PopupManager.VideoPlayer(Singleton<WebRequestManager>.instance.GetMediaPath(localResData.name, true));
        }
    }
}
