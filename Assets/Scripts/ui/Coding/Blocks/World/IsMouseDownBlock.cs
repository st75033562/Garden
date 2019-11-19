using System.Collections;
using UnityEngine;

public class IsMouseDownBlock : BlockBehaviour
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        retValue.value = CodeContext.input.GetMouseButton(0).ToString();
        yield break;
    }
}
