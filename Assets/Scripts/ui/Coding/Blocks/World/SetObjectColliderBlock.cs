using System.Collections;

public class SetObjectColliderBlock : BlockBehaviour
{
    private DownMenuPlugins[] m_menus;

    protected override void Start()
    {
        base.Start();

        m_menus = GetComponentsInChildren<DownMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var data = CodeContext.variableManager.get<VariableData>(m_menus[0].GetMenuValue());
        if (data != null)
        {
            var isTrigger = m_menus[1].GetMenuValue() == "down_menu_collider_trigger";
            CodeContext.worldApi.SetTrigger((int)data.getValue(), isTrigger);
        }
        yield break;
    }
}
