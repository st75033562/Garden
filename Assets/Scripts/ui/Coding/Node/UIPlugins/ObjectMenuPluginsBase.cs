using System.Linq;
using UnityEngine;
using DataAccess;

public abstract class ObjectMenuPluginsBase : DownMenuWithHintPlugins
{
    public int assetId
    {
        get;
        private set;
    }

    protected override void OnInput(string str)
    {
        assetId = int.Parse(str);
        var asset = objectDataSource.GetAsset(assetId);
        base.OnInput(asset.localizedName);
    }

    public override void Clicked()
    {
        SetMenuItems(objectDataSource.objectResources.Select(x => {
            return new UIMenuItem(x.localizedName, x.id.ToString());
        }));
        base.Clicked();
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);
        assetId = (other as ObjectMenuPluginsBase).assetId;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        BundleAssetData asset;
        // for backward compatibility
        if (save.PluginTextValue != string.Empty)
        {
            asset = objectDataSource.GetAsset(save.PluginTextValue);
            if (asset == null)
            {
                Debug.LogError("invalid object localization key: " + save.PluginTextValue);
                asset = objectDataSource.objectResources.FirstOrDefault();
            }
        }
        else
        {
            asset = objectDataSource.GetAsset(save.PluginIntValue);
        }

        if (asset == null)
        {
            assetId = 0;
            SetPluginsText("variable_menu_click_to_choose");
        }
        else
        {
            assetId = asset.id;
            SetPluginsText(asset.localizedName);
        }
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        var saveData = base.GetPluginSaveData();
        // we don't need text value
        saveData.PluginTextValue = string.Empty;
        saveData.PluginIntValue = assetId;
        return saveData;
    }

    protected abstract IObjectResourceDataSource objectDataSource { get; }
}
