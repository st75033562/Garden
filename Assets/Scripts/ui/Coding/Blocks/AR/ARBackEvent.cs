using System.Collections;

public class ARBackEvent : InsertBlock
{
	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
        retValue.value = "1";
        yield break;
	}
}
