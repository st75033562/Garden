using System.Collections;

public class ARFrontEvent : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = "0";
        yield break;
    }
}
