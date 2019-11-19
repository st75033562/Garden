using System.Collections;
using System.Linq;

public class ObjectCollisionOccurred : InsertBlock
{
    private DownMenuPlugins m_varMenu;

    protected override void Start()
    {
        base.Start();

        m_varMenu = GetComponentInChildren<DownMenuPlugins>(true);
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var objectId = CodeContext.variableManager.getVarInt(m_varMenu.GetMenuValue());
        if (objectId != 0)
        {
            var objectIds = CodeContext.worldApi.GetObjectCollidedObjects(objectId, false);
            retValue.value = objectIds.Any().ToString();
        }
        else
        {
            retValue.value = false.ToString();
        }
        yield break;
    }
}
