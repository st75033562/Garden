using DataAccess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoardSelectItemData
{
    public IRepositoryPath path;
    public PathInfo pathInfo;
    public int themeId;
}

public class GameBoardSelectItem : ScrollCell {
    [SerializeField]
    private Text textName;
    [SerializeField]
    private AssetBundleSprite assetBundleSprite;
    public Image imageIcon;

    public GameBoardSelectItemData Data
    {
        get { return (GameBoardSelectItemData)DataObject; }
    }

    public override void configureCellData() {
        textName.text = Data.path.name;
        if(Data.path.isDir) {
            imageIcon.gameObject.SetActive(true);
            assetBundleSprite.gameObject.SetActive(false);
        } else {
            assetBundleSprite.gameObject.SetActive(true);
            imageIcon.gameObject.SetActive(false);

            GameboardTemplateData gbThemeData = GameboardTemplateData.Get(Data.themeId);
            assetBundleSprite.SetAsset(gbThemeData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
        }
    }

    public void OnClick() {
        IsSelected = true;
        (Context as PopupGameBoardSelect).ClickCell(Data.path);
    }
}
