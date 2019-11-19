using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public class GameBoardThemeCell : ScrollCell
{
    public Text textName;
    public AssetBundleSprite bundleSprite;

    // Use this for initialization
    void Start()
    {

    }

    public GameboardThemeData gameBoardThemeData
    {
        get { return DataObject as GameboardThemeData; }
    }

    public override void configureCellData()
    {
        if (DataObject == null)
            return;

        textName.text = gameBoardThemeData.name.Localize();
        bundleSprite.SetAsset(gameBoardThemeData.thumbnailBundleName, GameboardTemplateData.Thumbnail);
    }

    public void OnClick()
    {
        ((PopupGameBoardTheme)Context).OnClickThemeCell(this);
    }
}