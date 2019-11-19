using System.Collections;

public class AttachObjectToRobotBlock : RobotBlockBase
{
    private DataMenuPlugins m_objectVarMenu;

    protected override void Start()
    {
        base.Start();

        m_objectVarMenu = GetComponentInChildren<DataMenuPlugins>();
    }

    protected override IEnumerator DoAction(BlockState state)
    {
        var sourceObjId = CodeContext.variableManager.getVarInt(m_objectVarMenu.GetMenuValue());
        CodeContext.worldApi.AttachObjectToRobot(sourceObjId, state.robotIndex);

        yield break;
    }
}
