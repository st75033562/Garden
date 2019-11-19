using UnityEngine;

public class LocalizedResType
{
    public const string Sound = "Sounds";
    public const string Image = "Images";
    public const string String = "Strings";
}

public static class LocalizationManagerExtensions
{
    public static AudioClip getSound(this LocalizationManager manager, string name)
    {
        return manager.loadResource<AudioClip>(LocalizedResType.Sound, name);
    }

    public static Sprite getSprite(this LocalizationManager manager, string name)
    {
        return manager.loadResource<Sprite>(LocalizedResType.Image, name);
    }
}
