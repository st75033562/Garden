using System.Collections;

public class FunctionDeclarationBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        for (var curNode = Node.NextNode; curNode && !context.isReturned; curNode = curNode.NextNode)
        {
            yield return curNode.ActionBlock(context);
        }
    }
}
