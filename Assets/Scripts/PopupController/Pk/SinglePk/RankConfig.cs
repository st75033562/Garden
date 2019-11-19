using System;
using UnityEngine;

public class RankConfig : ScriptableObject
{
    [SerializeField]
    private Sprite[] m_sprites;

    [SerializeField]
    private Color[] m_colors;

    [SerializeField]
    private Color m_defaultColor;

    public Sprite GetSprite(int rank)
    {
        if (rank < 1)
        {
            throw new ArgumentOutOfRangeException("rank");
        }
        return rank <= m_sprites.Length ? m_sprites[rank - 1] : null;
    }

    public Color GetColor(int rank)
    {
        return 1 <= rank && rank <= m_colors.Length ? m_colors[rank - 1] : m_defaultColor;
    }
}
