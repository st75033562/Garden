using System;
using UnityEngine.UI;

public class CodeSettingsView : SceneController
{
    public Text textAutoStop;

    protected override void Start()
    {
        base.Start();

        UpdateAutoStop();
    }

    private void UpdateAutoStop()
    {
        switch (Preference.autoStop)
        {
            case AutoStop.Immediate:
                textAutoStop.SetLocText("setting_auto_stop_immediately");
                break;
            case AutoStop.AfterOneSec:
                textAutoStop.SetLocText("setting_auto_stop_after_a_second");
                break;
            case AutoStop.SustainPlaying:
                textAutoStop.SetLocText("setting_auto_stop_sustain");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnClickAutoStop()
    {

    }
}
