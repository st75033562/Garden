using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image foregroundImage;
    public Text hintText;
    public Text progressText;

    // TOOD: add support for sliced sprite
    public float progress
    {
        get { return foregroundImage.fillAmount; }
        set
        {
            foregroundImage.fillAmount = Mathf.Clamp01(value);
            if (progressText)
            {
                progressText.text = (int)(foregroundImage.fillAmount * 100) + " %";
            }
        }
    }

    public string hint
    {
        get { return hintText != null ? hintText.text : string.Empty; }
        set
        {
            if (hintText)
            {
                hintText.text = value;
            }
        }
    }
}
