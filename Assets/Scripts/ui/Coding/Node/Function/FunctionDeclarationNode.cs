using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionDeclarationNodeState
{
    public readonly List<int> paramNodeIndices = new List<int>();

    public bool Validate(FunctionDeclaration decl)
    {
        if (decl == null)
        {
            throw new ArgumentNullException("decl");
        }

        return paramNodeIndices.Count == decl.parts.Count(x => x.type != FunctionPartType.Label);
    }
}

public class FunctionDeclarationNode : FunctionNode
{
    public FunctionPartResources m_PartResources;

    public FunctionDeclaration Declaration { get; private set; }

    public FunctionDeclarationNodeState GetState()
    {
        var state = new FunctionDeclarationNodeState();
        for (int i = 0; i < SlotPluginsCount; ++i)
        {
            state.paramNodeIndices.Add(GetSlotPlugin(i).InsertedNode.NodeIndex);
        }
        return state;
    }

    public void Rebuild(FunctionDeclaration declaration, FunctionDeclarationNodeState state = null)
    {
        if (declaration == null)
        {
            throw new ArgumentNullException("declaration");
        }

#if UNITY_EDITOR
        if (state != null && !state.Validate(declaration))
        {
            throw new ArgumentException("state is invalid for declaration");
        }
#endif

        Declaration = declaration;

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            Children[i].Delete(false);
        }

        for (int i = Plugins.Count - 1; i >= 0; --i)
        {
            var plugin = Plugins[i];
            RemovePluginAt(i);
            Destroy(plugin.gameObject);
        }

        int paramIndex = 0;
        foreach (var part in declaration.parts)
        {
            var pluginRes = m_PartResources.parts[(int)part.type].gameObject;
            var plugin = Instantiate(pluginRes, GetPluginRoot(), false).GetComponent<NodePluginsBase>();
            AddPlugins(plugin);
            if (part.type == FunctionPartType.Label)
            {
                plugin.SetPluginsText(part.text);
            }
            else
            {
                var templateId = m_PartResources.argNodeTemplateIds[(int)part.type];
                var paramNode = (FunctionArgumentNode)NodeTemplateCache.Instance.GetTemplate(templateId).Clone(transform);
                paramNode.TemplateHasState = true;
                // enable copying for the node
                paramNode.IsTemplate = true;
                paramNode.ArgName = part.text;
                paramNode.IsTransient = true;

                if (state != null)
                {
                    paramNode.NodeIndex = state.paramNodeIndices[paramIndex];
                }

                var slot = (SlotPlugins)plugin;
                slot.Insert(paramNode);
                // prevent users from changing the parameter node
                slot.Insertable = false;

                if (CodePanel)
                {
                    CodePanel.AddNode(paramNode, state == null);
                }

                ++paramIndex;
            }
        }

        LayoutTopDown();
    }

    protected override void OnAddedToCodePanel()
    {
        foreach (var child in Children)
        {
            CodePanel.AddNode(child);
        }
    }

    protected override IMessage PackNodeSaveData()
    {
        var saveData = new Save_FunctionDeclNode();
        saveData.BaseData = PackBaseNodeSaveData();
        saveData.FunctionId = ProtobufUtils.ToByteString(Declaration.functionId);
        return saveData;
    }

    protected override void UnPackNodeSaveData(byte[] nodeData)
    {
        var saveData = Save_FunctionDeclNode.Parser.ParseFrom(nodeData);
        UnPackBaseNodeSaveData(saveData.BaseData);
    }

    internal override void PostClone(FunctionNode other)
    {
        base.PostClone(other);

        Declaration = (other as FunctionDeclarationNode).Declaration;
    }
}
