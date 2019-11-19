using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionCallBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        yield return Call(context, null);
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        yield return Call(context, retValue);
    }

    private IEnumerator Call(ThreadContext context, ValueWrapper<string> retValue)
    {
        var callNode = (FunctionCallNode)Node;
        var declNode = Node.CodePanel.GetFunctionNode(callNode.Declaration.functionId);
        if (declNode)
        {
            var arguments = new Dictionary<string, string>();
            yield return GetArguments(context, declNode.Declaration, arguments);

            var record = new ActiveRecord(declNode.Declaration.functionId, arguments);
            if (context.Push(record))
            {
                yield return declNode.ActionBlock(context);
                context.Pop();
            }

            if (retValue != null)
            {
                retValue.value = Node.Insertable ? record.returnValue : "";
            }
        }
        else
        {
            Debug.LogError("cannot find the declaration node");
        }
    }

    private IEnumerator GetArguments(
        ThreadContext context, FunctionDeclaration decl, Dictionary<string, string> arguments)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int i = 0;
        foreach (var parameter in decl.parameters)
        {
            arguments.Add(parameter.text, slotValues[i++]);
        }
    }
}
