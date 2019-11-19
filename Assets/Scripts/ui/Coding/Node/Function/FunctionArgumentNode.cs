using Google.Protobuf;
using System;
using UnityEngine.UI;

public class FunctionArgumentNode : FunctionNode
{
    public string ArgName
    {
        get { return Plugins[0].GetPluginsText(); }
        set { Plugins[0].SetPluginsText(value); }
    }

    protected override IMessage PackNodeSaveData()
    {
        var saveData = new Save_FunctionArgNode();
        saveData.BaseData = PackBaseNodeSaveData();
        saveData.ParentNodeIndex = GetParentNodeIndex();
        saveData.BaseData.PluginList[0].PluginTextValue = ArgName;
        return saveData;
    }

    protected override void UnPackNodeSaveData(byte[] nodeData)
    {
        var saveData = Save_FunctionArgNode.Parser.ParseFrom(nodeData);
        UnPackBaseNodeSaveData(saveData.BaseData);
        ArgName = saveData.BaseData.PluginList[0].PluginTextValue;
        m_ParentIndexInSave = saveData.ParentNodeIndex;
    }
}
