using System;
using UnityEngine;

public static class ClassIconResource
{
    private static SpriteCollections s_icons;

    public static void Initialize()
    {
        s_icons = Resources.Load<SpriteCollections>("class_icons");
    }

    public static Sprite GetIcon(int id)
    {
        if (id < 0 || id >= s_icons.sprites.Length)
        {
            throw new ArgumentOutOfRangeException("id");
        }
        return s_icons.sprites[id];
    }
}
