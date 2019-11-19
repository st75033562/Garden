using UnityEngine;
using UnityEngine.UI;

public class GrayColorPlugins : NodePluginsBase
{
    public Image m_colorImage;

    public float brightness
    {
        get { return m_colorImage.color.r; }
        set
        {
            var color = m_colorImage.color;
            color.r = color.g = color.b = Mathf.Clamp01(value);
            m_colorImage.color = color;
        }
    }

    public float alpha
    {
        get { return m_colorImage.color.a; }
        set
        {
            var color = m_colorImage.color;
            color.a = Mathf.Clamp01(value);
            m_colorImage.color = color;
        }
    }

    protected override void OnInput(string str)
    {
        var settings = GrayColorSettings.Parse(str);
        brightness = settings.brightness;
        alpha = settings.alpha;
    }

    public override void Clicked()
    {
        var config = new GrayColorSettings(brightness, alpha);
        var dialog = UIDialogManager.g_Instance.GetDialog<UIGrayColorDialog>();
        dialog.Configure(config, this);
        OpenDialog(dialog);
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        var saveData = base.GetPluginSaveData();
        saveData.PluginTextValue = new GrayColorSettings(brightness, alpha).ToString();
        return saveData;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        var setting = GrayColorSettings.Parse(save.PluginTextValue);
        brightness = setting.brightness;
        alpha = setting.alpha;

        save.PluginTextValue = "";
        base.LoadPluginSaveData(save);
    }
}
