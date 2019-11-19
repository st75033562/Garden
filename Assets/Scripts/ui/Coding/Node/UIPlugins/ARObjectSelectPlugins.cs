using UnityEngine.Assertions;
using DataAccess;

public class ARObjectSelectPlugins : NodePluginsBase
{
    protected override void Start()
    {
        base.Start();
        if (objectId == 0)
        {
            SetObjectId(Constants.DefaultARObjectId);
            LayoutChanged();
        }
    }

    private void SetObjectId(int objectId)
    {
        this.objectId = objectId;
        var objectData = ARObjectDataSource.GetObject(objectId);
        SetPluginsText(objectData.localizedName);
    }

    public int objectId
    {
        get;
        private set;
    }

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIARObjectDialog>();
        dialog.Configure(UserManager.Instance.arObjects, new RemoteARObjectService(), this);
        OpenDialog(dialog);
    }

    protected override void OnInput(string str)
    {
        var newObjId = int.Parse(str);
        if (newObjId != this.objectId)
        {
            SetObjectId(newObjId);
            MarkChanged();
        }
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        var objectData = ARObjectDataSource.GetObject(save.PluginIntValue);
        if (objectData == null)
        {
            objectData = ARObjectDataSource.GetObject(Constants.DefaultARObjectId);
        }
        SetPluginsText(objectData.localizedName);
        objectId = objectData.id;
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        var data = base.GetPluginSaveData();
        data.PluginIntValue = objectId;
        return data;
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);
        objectId = ((ARObjectSelectPlugins)other).objectId;
    }
}
