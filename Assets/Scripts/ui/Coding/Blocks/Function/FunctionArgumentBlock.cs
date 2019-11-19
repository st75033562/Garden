using System.Collections;

public class FunctionArgumentBlock : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var argNode = (FunctionArgumentNode)Node;
        retValue.value = context.GetArgument(argNode.ArgName) ?? "";
        yield break;
    }
}
