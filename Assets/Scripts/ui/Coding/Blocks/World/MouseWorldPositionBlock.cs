using System;
using System.Collections;

public abstract class MouseWorldPositionBlock : BlockBehaviour
{
    public abstract int axis
    {
        get;
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = CodeContext.gameboardService.mouseWorldPosition[axis].ToString();
        yield break;
    }
}

public class MouseWorldPositionXBlock : MouseWorldPositionBlock
{
    public override int axis
    {
        get { return 0; }
    }
}

public class MouseWorldPositionYBlock : MouseWorldPositionBlock
{
    public override int axis
    {
        get { return 1; }
    }
}
