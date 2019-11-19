using Robomation;
using UnityEngine;

public class UIMonitorDialog : UIInputDialogBase
{
    public UIMonitor m_Monitor;
	public GameObject m_PhoneMonitor;

    public void Configure(IRobotManager robotManager, VariableManager varManager, bool showPhoneMonitor)
    {
        m_Monitor.Init(robotManager, varManager);
        m_PhoneMonitor.SetActive(showPhoneMonitor);
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIMonitorDialog; }
    }
}
