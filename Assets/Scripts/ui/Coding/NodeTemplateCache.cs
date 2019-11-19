using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using DataAccess;
using System.Reflection;
using UnityEngine.UI;

/// <summary>
/// To optimize loading of coding scene
/// NOTE: Canvas is required for properly calculating the width of Text and must be enabled
/// </summary>
[RequireComponent(typeof(Canvas))]
public class NodeTemplateCache : MonoBehaviour
{
    public event Action<float> onInitProgressChanged;

    public GameObject[] m_Node;
    public GameObject[] m_Plugins;

    private readonly Dictionary<int, FunctionNode> m_Nodes = new Dictionary<int, FunctionNode>();
    private readonly HashSet<int> m_disabledIds = new HashSet<int>();
    private Dictionary<string, List<string>> nodeInitState = new Dictionary<string, List<string>>();

    // copy of plugin ids for reconfiguring when language is changed
    private readonly Dictionary<int, string> m_pluginConfigs = new Dictionary<int, string>();

    private static NodeTemplateCache s_instance;

    public static NodeTemplateCache Instance
    {
        get { return s_instance; }
    }

    void Awake()
    {
        Assert.IsNull(s_instance);
        s_instance = this;
    }

    public Coroutine Init(bool async = true)
    {
        return StartCoroutine(CreateTemplates(async));
    }

    public FunctionNode MainNode { get; private set; }

    public IEnumerable<FunctionNode> Templates
    {
        get { return m_Nodes.Values; }
    }

    private bool showBlockUI = true;

    public FunctionNode GetTemplate(int id)
    {
        FunctionNode node;
        m_Nodes.TryGetValue(id, out node);
        return node;
    }

    public bool IsNodeEnabled(int index)
    {
        return !m_disabledIds.Contains(index);
    }

    IEnumerator CreateTemplates(bool async)
    {
        ReportProgress(0.0f);

        int i = 0;
        foreach (var template in NodeTemplateData.Data)
        {
            m_pluginConfigs.Add(template.id, template.pluginConfig);
            var newNode = CreateTemplate(template);
            m_Nodes.Add(newNode.NodeTemplateId, newNode);
            if (!template.enabled)
            {
                m_disabledIds.Add(newNode.NodeTemplateId);
            }

            ReportProgress((i + 1) / (float)NodeTemplateData.Count);
            if (async)
            {
                yield return null;
                // deactivate the object after Start is called
                newNode.gameObject.SetActive(false);
            }

            ++i;
        }
    }

    private FunctionNode CreateTemplate(NodeTemplateData template)
    {
        // instantiate the block off the screen
        var nodeObj = Instantiate(m_Node[template.resId], new Vector3(Screen.width, 0, 0), Quaternion.identity, transform);
        nodeObj.name = template.name;

        var nodeFunc = nodeObj.GetComponent<FunctionNode>();
        nodeFunc.NodeCategory = (NodeCategory)template.type;
        nodeFunc.NodeTemplateId = template.id;

        foreach (var pluginId in template.GetPluginIds())
        {
            var pluginData = NodePluginData.Get(pluginId);
            var pluginObj = (GameObject)Instantiate(m_Plugins[pluginData.resId], nodeFunc.GetPluginRoot(), false);
            var plugin = pluginObj.GetComponent<NodePluginsBase>();
            if (pluginData.extension != "")
            {
                try
                {
                    plugin = DoPluginsExtend(nodeObj, plugin, pluginData.extension);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to configure plugin " + pluginId);
                    Debug.LogException(e);
                    throw;
                }
            }
            if (pluginData.clickAction != "")
            {
                plugin.DecodeClickedCMD(pluginData.clickAction);
            }
            plugin.PluginID = pluginId;

            nodeFunc = nodeObj.GetComponent<FunctionNode>();
            nodeFunc.AddPlugins(plugin);
        }

        if (template.scriptName != "")
        {
            Type t = Type.GetType(template.scriptName);
            if (null != t)
            {
                nodeObj.AddComponent(t);
            }
            else
            {
                Debug.LogError("invalid node script: " + template.scriptName);
            }

            if ("WhenRunClickedBlock" == template.scriptName && null == MainNode)
            {
                MainNode = nodeFunc;
            }
        }
        nodeFunc.Layout();

        return nodeFunc;
    }

    private void ReportProgress(float progress)
    {
        if (onInitProgressChanged != null)
        {
            onInitProgressChanged(progress);
        }
    }

    NodePluginsBase DoPluginsExtend(GameObject nodeObj, NodePluginsBase plugin, string extend)
    {
        string[] cmds = extend.Split(new string[] { "```" }, StringSplitOptions.RemoveEmptyEntries);
        // make sure Script:: come first, because plugin will be replaced, 
        // we should not add the destroyed plugin to StepNode or SlotNode
        int index = Array.FindIndex(cmds, x => x.StartsWith("Script"));
        if (index != -1)
        {
            cmds.Swap(0, index);
        }

        for (int i = 0; i < cmds.Length; ++i)
        {
            string[] extends = cmds[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            if ("Text" == extends[0])
            {
                string text = "";
                if (extends.Length > 1)
                {
                    text = extends[1];
                }
                plugin.SetPluginsText(text);
            }
            else if ("XOffSet" == extends[0])
            {
                float offset = float.Parse(extends[1]);
                plugin.PosOffset = new Vector2(offset, plugin.PosOffset.y);
            }
            else if ("YOffSet" == extends[0])
            {
                float offset = float.Parse(extends[1]);
                plugin.PosOffset = new Vector2(plugin.PosOffset.x, offset);
            }
            else if ("WOffSet" == extends[0])
            {
                float offset = float.Parse(extends[1]);
                plugin.SizeOffset = new Vector2(offset, plugin.SizeOffset.y);
            }
            else if ("HOffSet" == extends[0])
            {
                float offset = float.Parse(extends[1]);
                plugin.SizeOffset = new Vector2(plugin.SizeOffset.x, offset);
            }
            else if ("Step" == extends[0])
            {
                StepNode mNode = nodeObj.GetComponent<StepNode>();
                mNode.AddPluginsToStep(plugin, int.Parse(extends[1]) - 1);
            }
            else if ("Script" == extends[0])
            {
                var fields = extends[1].Split('=');
                var scriptName = fields[0];
                var attributes = fields.Length > 1 ? fields[1] : null;

                if (scriptName != string.Empty && plugin.GetType().Name != scriptName)
                {
                    Type t = Type.GetType(scriptName);
                    if (null != t)
                    {
                        // deactivate the object to delay the call of Awake for the new script
                        // until all necessary data is copied over to the new script
                        plugin.gameObject.SetActive(false);
                        var newPlugin = plugin.gameObject.AddComponent(t) as NodePluginsBase;
                        plugin.CopyDataToTarget(newPlugin);
                        // now it's safe to call Awake for the new script
                        plugin.gameObject.SetActive(true);
                        DestroyImmediate(plugin);
                        plugin = newPlugin;

                        if (attributes != null)
                        {
                            ApplyJsonAttribute(newPlugin, attributes);
                        }
                    }
                }
                else if (attributes != null)
                {
                    ApplyJsonAttribute(plugin, attributes);
                }
            }
        }
        return plugin;
    }

    void ApplyJsonAttribute(object obj, string str)
    {
        var jsonData = JsonMapper.ToObject(str);
        var type = obj.GetType();
        foreach (var propName in jsonData.Keys)
        {
            var prop = type.GetProperty(propName);
            var value = jsonData[propName];
            switch (value.GetJsonType())
            {
            case JsonType.Boolean:
                prop.SetValue(obj, (bool)value, null);
                break;

            case JsonType.Double:
                prop.SetValue(obj, (float)(double)value, null);
                break;

            case JsonType.Int:
                prop.SetValue(obj, (int)value, null);
                break;

            case JsonType.Long:
                prop.SetValue(obj, (long)value, null);
                break;

            case JsonType.String:
                prop.SetValue(obj, (string)value, null);
                break;

            default:
                throw new ArgumentException("unsupported property type: " + value.GetJsonType());
            }
        }
    }

    public void Refresh()
    {
        if (m_pluginConfigs.Count == 0)
        {
            return;
        }

        var newNodes = new List<FunctionNode>();
        // re-create template for plugins whose configuration has changed
        // for other plugins, doing a layout is enough
        foreach (var template in NodeTemplateData.Data)
        {
            var node = m_Nodes[template.id];
            if (template.pluginConfig == m_pluginConfigs[template.id])
            {
                node.RefreshPluginText();
            }
            else
            {
                //Debug.Log("recreate template: " + nodeData[i].m_NodeID);
                m_pluginConfigs[template.id] = template.pluginConfig;
                Destroy(node.gameObject);
                m_Nodes[template.id] = node = CreateTemplate(template);
                newNodes.Add(node);
            }
            node.Layout();
        }
    }

    public void LoadNodeInitState() {
        if (nodeInitState.Count > 0) {
            return;
        }
        foreach (var node in m_Nodes.Values)
        {
            List<string> acitiveChilds = new List<string>();
            foreach (Transform child in node.transform)
            {
                if (child.gameObject.activeSelf) {
                    acitiveChilds.Add(child.name);
                }
            }
            nodeInitState.Add(node.name, acitiveChilds);
        }
    }

    public bool ShowBlockUI {
        set {
            if (showBlockUI != value) {
                showBlockUI = value;
                foreach (var node in m_Nodes.Values)
                {
                    List<string> acitiveChilds = nodeInitState[node.name];
                    foreach (Transform child in node.transform)
                    {
                        if (showBlockUI)
                        {
                            if (acitiveChilds.Contains(child.name)) {
                                child.gameObject.SetActive(true);
                            }
                        }
                        else {
                            child.gameObject.SetActive(showBlockUI);
                        }
                    }
                }
            }
        }
        get {
            return showBlockUI;
        }
    }
}
