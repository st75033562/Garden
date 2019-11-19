using DataAccess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoardTemplateCell : ScrollCell
{
    public Text textName;
    public AssetBundleSprite bundleSprite;

    // Use this for initialization
    void Start()
    {

    }

    public GameboardTemplateData gameBoardTemplateData
    {
        get { return DataObject as GameboardTemplateData; }
    }

    public override void configureCellData()
    {
        if (gameBoardTemplateData == null)
            return;

        textName.text = gameBoardTemplateData.name.Localize();
        bundleSprite.SetAsset(gameBoardTemplateData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
    }

    public void OnClick()
    {
        ((PopupGameBoardTheme)Context).OnClickTemplateCell(this);
    }
}
