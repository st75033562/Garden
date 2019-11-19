using DataAccess;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIAssetCell : ScrollableCell
    {
        public AssetBundleSprite m_sprite;
        public Text m_assetNameText;

        public override void ConfigureCellData()
        {
            m_sprite.SetAsset(data.bundleName, data.thumbnailAssetName);
            m_assetNameText.text = data.localizedName.Localize();
        }

        public BundleAssetData data
        {
            get { return (BundleAssetData)dataObject; }
        }
    }
}
