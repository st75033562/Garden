using System.Collections;

public class GetRobotTransformBlock : RobotBlockBase
{
    private VariableMenuPlugins[] m_variableMenus;

    protected override void Start()
    {
        base.Start();

        m_variableMenus = GetComponentsInChildren<VariableMenuPlugins>();
    }

    protected override IEnumerator DoAction(BlockState state)
    {
        var pos = CodeContext.worldApi.GetRobotPosition(state.robotIndex);
        SetVariable(0, pos.x);
        SetVariable(1, pos.y);
        SetVariable(2, CodeContext.worldApi.GetRobotRotation(state.robotIndex));

        yield break;
    }

    private void SetVariable(int index, float value)
    {
        CodeContext.variableManager.setVar(m_variableMenus[index].GetMenuValue(), value);
    }
}