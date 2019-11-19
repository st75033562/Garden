public class ARMarkerSelectPlugins : SlotPlugins
{
    public int GetMarkerId()
    {
        int markerId;
        if (int.TryParse(GetPluginsText(), out markerId))
        {
            return markerId;
        }
        return -1;
    }

    public override void Clicked()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIARMarkerDialog>();
        dialog.Configure(this);
        OpenDialog(dialog);
    }
}
