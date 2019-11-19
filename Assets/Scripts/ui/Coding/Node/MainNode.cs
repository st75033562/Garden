using Google.Protobuf;
using System;
using System.Collections;
using UnityEngine;

// for identifying the root node
public class MainNode : FunctionNode
{
    internal bool TryStart()
    {
        if (CodePanel)
        {
            return CodePanel.TryStart(this);
        }

        return false;
    }
}
