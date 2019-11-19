using LitJson;
using UnityEngine;
using UnityEngine.UI;

public class GrayColorSettings
{
    private float m_brightness = 1.0f;
    private float m_alpha = 1.0f;

    public float brightness
    {
        get { return m_brightness; }
        set { m_brightness = Mathf.Clamp01(value); }
    }

    public float alpha
    {
        get { return m_alpha; }
        set { m_alpha = Mathf.Clamp01(value); }
    }

    public GrayColorSettings() { }

    public GrayColorSettings(float brightness, float alpha)
    {
        this.brightness = brightness;
        this.alpha = alpha;
    }

    public override string ToString()
    {
        return JsonMapper.ToJson(this);
    }

    public static GrayColorSettings Parse(string str)
    {
        return JsonMapper.ToObject<GrayColorSettings>(str);
    }
}

public class UIGrayColorDialog : UIInputDialogBase
{
    public Slider m_brightnessSlider;
    public Slider m_alphaSlider;
    public Text brightnessProgress;
    public Text alphaProgress;

    private IDialogInputCallback m_callback;

    public void Configure(GrayColorSettings config, NodePluginsBase plugin)
    {
        m_callback = plugin;
        m_brightnessSlider.value = config.brightness;
        m_alphaSlider.value = config.alpha;
        UpdateProgress();
    }

    public override void CloseDialog()
    {
        if (m_callback != null)
        {
            var result = new GrayColorSettings {
                brightness = m_brightnessSlider.value,
                alpha = m_alphaSlider.value
            };
            m_callback.InputCallBack(result.ToString());
        }
        base.CloseDialog();
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIGrayColorDialog; }
    }

    private void Update()
    {
        UpdateProgress();
    }

    void UpdateProgress()
    {
        brightnessProgress.text = ((int)(m_brightnessSlider.value * 100)) + "%";
        alphaProgress.text = ((int)(m_alphaSlider.value * 100)) + "%";
    }
}
