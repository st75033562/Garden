using System;
using UnityEngine;

public class FunctionPartResources : ScriptableObject
{
    // corresponds to FunctionPartType
    public NodePluginsBase[] parts;
    public string[] partConfigs;
    public string[] partDefaults;

    // corresponds to FunctionPartType
    // no template for Label
    public int[] argNodeTemplateIds;
}
