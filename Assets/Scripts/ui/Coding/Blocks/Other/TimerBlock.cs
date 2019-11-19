using System.Collections;

public class TimerBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = CodeContext.timer.elapsedTime.ToString();
        yield break;
    }
}
