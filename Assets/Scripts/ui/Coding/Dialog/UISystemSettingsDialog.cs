using System;
using UnityEngine;
using UnityEngine.UI;

public interface ISystemSettingsDialogViewMode
{
    bool SoundEnabled { get; set; }
    BlockLevel BlockLevel { get; set; }
    AutoStop StopMode { get; set; }

    bool StopModeVisible { get; }

    void OnClosed();
}

public class UISystemSettingsDialog : UIInputDialogBase
{
    public Toggle m_SoundToggle;
	public Text m_StopState;
	public Text m_LevelState;
    public RectTransform m_StopModeTrans;
    public RectTransform m_LevelTrans;
	public UIMenuWidget m_Menu;
    public GameObject m_StopModeGo;

    private ISystemSettingsDialogViewMode m_viewModel;

	public void OnSoundToggled(bool isOn)
	{
        m_viewModel.SoundEnabled = isOn;
    }

	public void SelectStopMode()
	{
        m_Menu.onItemClicked.AddListener(OnSelectStopMode);
        m_Menu.SetOptions(new[] {
            "setting_stop_immediate".Localize(),
            "setting_stop_afteronesec".Localize(),
            "setting_stop_sustainplaying".Localize()
        });
        m_Menu.onItemClicked.AddListener(OnSelectStopMode);
        OpenMenu(m_StopModeTrans);
	}

	public void SelectLevel()
	{
        m_Menu.SetOptions(new[] {
            "setting_level_1".Localize(), 
            "setting_level_2".Localize(), 
            "setting_level_3".Localize(), 
        });
		m_Menu.onItemClicked.AddListener(OnSelectLevel);
        OpenMenu(m_LevelTrans);
	}

    private void OpenMenu(RectTransform target)
    {
        var pos = target.TransformPoint(new Vector3(0, target.rect.yMin, 0));
        m_Menu.SetPosition(pos);
        m_Menu.Open();
    }

	void OnSelectStopMode(int index)
	{
        m_Menu.onItemClicked.RemoveListener(OnSelectStopMode);
        m_viewModel.StopMode = (AutoStop)index;
        UpdateAutoState();
    }

	void UpdateAutoState()
	{
        m_StopState.text = ("setting_stop_" + m_viewModel.StopMode.ToString().ToLower()).Localize();
	}

	void OnSelectLevel(int index)
	{
        m_Menu.onItemClicked.RemoveListener(OnSelectLevel);
        m_viewModel.BlockLevel = (BlockLevel)(index + 1);
        UpdateLevel();
	}

	void UpdateLevel()
	{
        m_LevelState.text = ("setting_level_" + (int)m_viewModel.BlockLevel).Localize();
	}

    public void Configure(ISystemSettingsDialogViewMode viewMode)
    {
        if (viewMode == null)
        {
            throw new ArgumentNullException("viewMode");
        }
        m_viewModel = viewMode;
    }

	public override void OpenDialog()
	{
        base.OpenDialog();

        m_SoundToggle.isOn = m_viewModel.SoundEnabled;
        m_StopModeGo.SetActive(m_viewModel.StopModeVisible);

		UpdateAutoState();
		UpdateLevel();
	}

    public override void CloseDialog()
    {
        base.CloseDialog();
        m_viewModel.OnClosed();
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UISystemSettingsDialog; }
    }
}
