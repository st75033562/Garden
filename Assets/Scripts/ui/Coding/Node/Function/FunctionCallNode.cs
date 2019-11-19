using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class DeletedArgInfo
{
    public readonly int pluginId;
    public readonly Save_PluginsData pluginState;
    public readonly BlockSaveStates insertedNodeStates;
    public readonly int connectionId;

    public DeletedArgInfo(int pluginId, Save_PluginsData pluginState, FunctionNode insertedNode, int connectionId)
    {
        this.pluginId = pluginId;
        this.pluginState = pluginState;
        insertedNodeStates = new BlockSaveStates(insertedNode.CodePanel.m_Workspace, new[] { insertedNode }, false);
        this.connectionId = connectionId;
    }
}

public class FunctionCallNode : FunctionNode
{
    public FunctionPartResources m_PartResources;

    public FunctionDeclaration Declaration { get; private set; }

    public List<DeletedArgInfo> Rebuild(FunctionDeclaration declaration)
    {
        if (declaration == null)
        {
            throw new ArgumentNullException("declaration");
        }

        Assert.IsTrue(!ReferenceEquals(declaration, Declaration));

        // save old plugins and insertions
        var oldDecl = Declaration;
        var labelPlugins = new Queue<NodePluginsBase>();
        var slotPlugins = new Dictionary<string, SlotPlugins>();
        var oldConnIds = new Dictionary<FunctionNode, int>();
        if (oldDecl != null)
        {
            for (int i = oldDecl.parts.Count - 1; i >= 0; --i)
            {
                var part = oldDecl.parts[i];
                if (Plugins[i] is SlotPlugins)
                {
                    var slot = Plugins[i] as SlotPlugins;
                    slotPlugins.Add(part.text, slot);
                    if (slot.InsertedNode)
                    {
                        oldConnIds.Add(slot.InsertedNode, slot.InsertedNode.GetPrevConnection().id);
                    }
                }
                else
                {
                    labelPlugins.Enqueue(Plugins[i]);
                }
                // keep the insertion to simplify the code a bit...
                RemovePluginAt(i, false);
            }
        }

        Declaration = declaration;
        for (int i = 0; i < declaration.parts.Count; ++i)
        {
            bool isNew = false;
            var part = declaration.parts[i];
            NodePluginsBase plugin = null;
            if (part.type == FunctionPartType.Label)
            {
                if (labelPlugins.Count > 0)
                {
                    plugin = labelPlugins.Dequeue();
                }
            }
            // keep the inserted node if possible
            else if (slotPlugins.ContainsKey(part.text) && oldDecl.GetPart(part.text).type == part.type)
            {
                plugin = slotPlugins[part.text];
                slotPlugins.Remove(part.text);
            }

            if (!plugin)
            {
                isNew = true;
                var pluginRes = m_PartResources.parts[(int)part.type].gameObject;
                plugin = Instantiate(pluginRes, GetPluginRoot(), false).GetComponent<NodePluginsBase>();

                var config = m_PartResources.partConfigs[(int)part.type];
                if (!string.IsNullOrEmpty(config))
                {
                    plugin.DecodeClickedCMD(config);
                }
            }
            // generate a unique id for the plugin so that the content can be saved correctly
            plugin.PluginID = i;
            AddPlugins(plugin);

            if (part.type == FunctionPartType.Label)
            {
                plugin.SetPluginsText(part.text);
            }
            else if (isNew)
            {
                plugin.SetPluginsText(m_PartResources.partDefaults[(int)part.type]);
            }
        }

        var deletedArgs = new List<DeletedArgInfo>();
        // remove old plugins and inserted nodes
        foreach (var slot in slotPlugins.Values)
        {
            if (slot.InsertedNode)
            {
                var insertedNode = slot.InsertedNode;
                slot.RemoveInsertion();
                deletedArgs.Add(new DeletedArgInfo(
                    slot.PluginID, slot.GetPluginSaveData(), insertedNode, oldConnIds[insertedNode]));
                insertedNode.Delete(false);
            }
            Destroy(slot.gameObject);
        }

        foreach (var plugin in labelPlugins)
        {
            Destroy(plugin.gameObject);
        }

        // reset connection ids so that Undo can work
        ResetConnectionIds();
        LayoutBottomUp();

        return deletedArgs;
    }

    protected override IMessage PackNodeSaveData()
    {
        var saveData = new Save_FunctionCallNode();
        saveData.BaseData = PackBaseNodeSaveData();
        saveData.FunctionId = ProtobufUtils.ToByteString(Declaration.functionId);
        return saveData;
    }

    protected override void UnPackNodeSaveData(byte[] nodeData)
    {
        var saveData = Save_FunctionCallNode.Parser.ParseFrom(nodeData);
        UnPackBaseNodeSaveData(saveData.BaseData);
    }

    internal override void PostClone(FunctionNode other)
    {
        base.PostClone(other);

        Declaration = (other as FunctionCallNode).Declaration;
    }
}
