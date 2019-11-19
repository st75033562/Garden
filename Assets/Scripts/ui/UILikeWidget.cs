using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILikeWidget : MonoBehaviour
{
    public float m_animDuration;
    public Sprite m_likedSprite;
    public Sprite m_likeHintSprite;
    public Sprite m_normalSprite;
    public Text m_likeCountText;
    public Image m_image;

    private bool m_liked;
    private int m_likeCount;

    private void Awake()
    {
        m_image.sprite = m_normalSprite;
    }

    public void SetLiked(bool liked, bool showAnim)
    {
        if (m_liked != liked)
        {
            StopAllCoroutines();
            m_liked = liked;
            if (m_liked)
            {
                if (showAnim)
                {
                    m_image.sprite = m_likeHintSprite;
                    StartCoroutine(ShowLiked());
                }
                else
                {
                    m_image.sprite = m_likedSprite;
                }
            }
            else
            {
                m_image.sprite = m_normalSprite;
            }
        }
    }

    public bool liked
    {
        get { return m_liked; }
    }

    public int likeCount
    {
        get { return m_likeCount; }
        set
        {
            m_likeCount = value;
            m_likeCountText.text = value.ToString();
        }
    }

    private IEnumerator ShowLiked()
    {
        yield return new WaitForSeconds(m_animDuration);
        m_image.sprite = m_likedSprite;
    }
}
