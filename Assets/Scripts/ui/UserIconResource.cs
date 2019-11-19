using System;
using UnityEngine;
using System.Collections.Generic;

public static class UserIconResource
{
    private static SpriteCollections s_Icons;

    public static void Initialize()
    {
        s_Icons = Resources.Load<SpriteCollections>("avatar_icons");
    }

    public static Sprite GetUserIcon(int id)
    {
        if(id < 0 || id >= s_Icons.sprites.Length) {
            throw new ArgumentOutOfRangeException("id");
        }
        return s_Icons.sprites[id];
    }
}
