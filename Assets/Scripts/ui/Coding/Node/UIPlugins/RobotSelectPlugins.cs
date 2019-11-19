public class RobotSelectPlugins : SlotPlugins
{
    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UINumSelectDialog>();
        var config = new UINumSelectDialogConfig() {
            title = "input_hardware_index",
            count = CodeContext.robotManager.robotCount
        };
        dialog.Configure(config, this);
        OpenDialog(dialog);
    }
}
