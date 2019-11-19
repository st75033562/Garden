using DataAccess;
using UnityEngine.UI;
using UnityEngine;

public class UIARObjectItemData
{
    public ARObjectData objectData;
    public bool unlocked;
}

public class UIARObjectItem : ScrollableCell
{
    public AssetBundleSprite m_thumbnail;
    public Text m_nameText;
    public GameObject m_lock;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        m_thumbnail.SetAsset(data.objectData.bundleName, data.objectData.thumbnailName);
        m_nameText.text = data.objectData.localizedName.Localize();
        m_lock.SetActive(!data.unlocked);
    }

    public UIARObjectItemData data
    {
        get { return (UIARObjectItemData)dataObject; }
    }
}