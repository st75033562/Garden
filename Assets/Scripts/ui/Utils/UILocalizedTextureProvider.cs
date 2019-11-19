using UnityEngine;

public class UILocalizedTextureProvider : UITextureProvider
{
    public string imagePath;

    public override Texture2D Get()
    {
        if (!string.IsNullOrEmpty(imagePath))
        {
            return LocalizationManager.instance.loadResource<Texture2D>(LocalizedResType.Image, imagePath);
        }
        return null;
    }
}
