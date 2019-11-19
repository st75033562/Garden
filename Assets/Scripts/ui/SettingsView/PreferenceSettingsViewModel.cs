public class PreferenceSettingsViewModel : ISystemSettingsDialogViewMode
{
    public bool SoundEnabled
    {
        get { return Preference.soundEffectEnabled; }
        set { Preference.soundEffectEnabled = value; }
    }

    public BlockLevel BlockLevel
    {
        get { return Preference.blockLevel; }
        set { Preference.blockLevel = value; }
    }

    public AutoStop StopMode
    {
        get { return Preference.autoStop; }
        set { Preference.autoStop = value; }
    }

    public bool StopModeVisible
    {
        get { return false; }
    }

    public void OnClosed()
    {
    }
}
