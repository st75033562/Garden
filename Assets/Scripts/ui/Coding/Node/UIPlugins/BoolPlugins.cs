public class BoolPlugins : SlotPlugins
{
    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIBoolSelectDialog>();
        dialog.Configure(this);
        OpenDialog(dialog);
    }
}
