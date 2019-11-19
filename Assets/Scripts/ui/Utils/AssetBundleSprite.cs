using System;
using UnityEngine;
using UnityEngine.UI;

public class AssetBundleSprite : AssetBundleObject
{
    public Image image;
    public GameObject loadingIcon;
    public Sprite defaultSprite;
    public bool setNativeSize;

    protected override void Awake()
    {
        base.Awake();

        image.enabled = false;
    }

    public void ShowDefaultSprite()
    {
        Unload();
        image.enabled = true;
        image.sprite = defaultSprite;
    }

    protected override void OnBeginLoad()
    {
        base.OnBeginLoad();

        if (loadingIcon)
        {
            loadingIcon.SetActive(true);
        }

        image.enabled = false;
    }

    protected override void OnEndLoad()
    {
        base.OnEndLoad();

        if (loadingIcon)
        {
            loadingIcon.SetActive(false);
        }

        image.enabled = true;
        if (setNativeSize)
        {
            image.SetNativeSize();
        }
    }

    protected override Type assetType
    {
        get { return typeof(Sprite); }
    }

    protected override void OnLoaded(UnityEngine.Object asset)
    {
        image.sprite = asset as Sprite ?? defaultSprite;
    }

    void Reset()
    {
        image = GetComponent<Image>();
    }
}
