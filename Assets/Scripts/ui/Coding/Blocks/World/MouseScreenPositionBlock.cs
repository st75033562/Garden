using System.Collections;

public class MouseScreenPositionXBlock : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = CodeContext.gameboardService.mouseScreenPosition[0].ToString();
        yield break;
    }
}

public class MouseScreenPositionYBlock : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = CodeContext.gameboardService.mouseScreenPosition[1].ToString();
        yield break;
    }
}
