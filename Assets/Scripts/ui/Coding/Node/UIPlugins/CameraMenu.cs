using RobotSimulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMenu : DownMenuWithHintPlugins
{

    public int assetId
    {
        get;
        private set;
    }
    private CameraBlockController rootCameraManager;

    protected override void OnInput(string str)
    {
        assetId = int.Parse(str);
     //   var asset = objectDataSource.GetAsset(assetId);
        base.OnInput(assetId.ToString());
    }

    public override void Clicked()
    {
        if (cameraManager != null)
        {
            List<UIMenuItem> list = new List<UIMenuItem>();
            for (int i = 0; i < cameraManager.cameras.Length; i++)
            {
                UIMenuItem item = new UIMenuItem("ui_text_camera".Localize() + i, i.ToString());
                list.Add(item);
            }
            SetMenuItems(list);
        }
        base.Clicked();
    }

    public override void InputCallBack(string str)
    {
        var oldState = GetPluginSaveData();

        OnInput(str);

        var newState = GetPluginSaveData();
        var args = new NodePluginChangedEvent(this, oldState, newState);
        CodeContext.eventBus.AddEvent(EventId.NodePluginChanged, args);
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);
        assetId = (other as CameraMenu).assetId;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {

        assetId = save.PluginIntValue;
        SetPluginsText("ui_text_camera".Localize() + assetId);
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        var saveData = base.GetPluginSaveData();
        // we don't need text value
        saveData.PluginTextValue = string.Empty;
        saveData.PluginIntValue = assetId;
        return saveData;
    }

    public CameraBlockController cameraManager
    {
        get {
            rootCameraManager = GameObject.FindObjectOfType<CameraBlockController>();
            if (rootCameraManager == null)
            {
                Debug.LogError("can not find root");
                return null;
            }
            else
            {
                return rootCameraManager;
            }
        }
    }
}
