using UnityEngine.UI;

public class LocalizedImage : LocalizedComponent
{
    public Image image;
    public string spriteName;

    void Reset()
    {
        image = GetComponent<Image>();
    }

    protected override void OnLanguageChanged()
    {
        image.sprite = LocalizationManager.instance.getSprite(spriteName);
    }
}
