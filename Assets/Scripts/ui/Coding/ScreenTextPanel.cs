using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PrintTextConfig
{
    public string text;
    public Vector2 pos;
    public int size;
    public Color color;
    public float bgBrightness;
    public float bgAlpha;
    public float duration;
}

// screen panel for printing text, needs to be on a standalone game object
public class ScreenTextPanel : MonoBehaviour
{
    public Transform root;
    public GameObject textTemplate;

    void Awake()
    {
        if (!root)
        {
            root = transform;
        }
    }

    public void Print(ref PrintTextConfig cfg)
    {
        var instance = Instantiate(textTemplate, root).GetComponent<ScreenText>(); ;
        instance.SetText(cfg.text, cfg.size, cfg.color);
        instance.backgroundBrightness = cfg.bgBrightness;
        instance.backgroundAlpha = cfg.bgAlpha;
        instance.transform.localPosition = new Vector3(cfg.pos.x, -cfg.pos.y, 0.0f);

        if (cfg.duration > 0)
        {
            Destroy(instance.gameObject, cfg.duration);
        }
    }

    public void Clear()
    {
        for (int i = root.childCount - 1; i >= 0; --i)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}