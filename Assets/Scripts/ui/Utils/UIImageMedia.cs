using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIImageMedia : MonoBehaviour
{
    public event Action<UIImageMedia> onImageLoaded; 

    public UITextureProvider m_defaultImageProvider;
    public Texture2D m_defaultImage;
    public RectTransform m_fitTarget;

    private RawImage m_rawImage;
    private string m_name;
    private Texture2D m_texture;
    private SimpleHttpRequest m_webRequest;
    private bool m_loadFromName;

    private AspectRatioFitter m_aspectRatioFitter;
    private bool m_initialized;

    void Awake()
    {
        if (!isLoaded) {
            UpdateTexture(defaultImage);
        }
    }

    private void Initialize()
    {
        if (m_initialized) { return; }

        m_initialized = true;
        if (!m_rawImage)
        {
            m_rawImage = GetComponent<RawImage>();
        }
        m_aspectRatioFitter = GetComponent<AspectRatioFitter>();
    }

    private Texture2D defaultImage
    {
        get
        {
            if (m_defaultImageProvider)
            {
                return m_defaultImageProvider.Get();
            }
            return m_defaultImage;
        }
    }

    void OnDestroy()
    {
        if (m_texture)
        {
            Destroy(m_texture);
        }

        CancelDownload();
    }

    private Texture2D texture
    {
        get
        {
            if (!m_texture)
            {
                m_texture = new Texture2D(0, 0);
            }
            return m_texture;
        }
    }

    public RawImage rawImage
    {
        get { return m_rawImage; }
    }

    public bool isLoaded
    {
        get;
        private set;
    }

    public void SetImage(byte[] data)
    {
        CancelDownload();
        PrepareForExternalTexture();
        UpdateTexture(data);
    }

    public void SetImage(Texture texture)
    {
        CancelDownload();
        PrepareForExternalTexture();
        UpdateTexture(texture);
    }

    private void PrepareForExternalTexture()
    {
        m_loadFromName = false;
        isLoaded = false;
        m_name = null;
    }

    public void SetImage(string name)
    {
        if (m_name == name && m_loadFromName)
        {
            if (isLoaded && onImageLoaded != null)
            {
                onImageLoaded(this);
            }
            return;
        }

        m_loadFromName = true;
        isLoaded = false;
        CancelDownload();

        m_name = name;
        if (!string.IsNullOrEmpty(name))
        {
            var data = LocalResCache.instance.LoadImage(m_name);
            if (data != null)
            {
                isLoaded = true;
                UpdateTexture(data);
                return;
            }
        }

        UpdateTexture(defaultImage);

        if (!string.IsNullOrEmpty(name))
        {
            m_webRequest = Downloads.DownloadMedia(name);
            m_webRequest.defaultErrorHandling = false;
            m_webRequest.Success(data => {
                isLoaded = true;
                m_webRequest = null;
                UpdateTexture(data);
                LocalResCache.instance.SaveImage(name, data);
            })
            .Error(() => {
                m_webRequest = null;
            })
            .Execute();
        }
    }

    private void UpdateTexture(byte[] data)
    {
        if (texture.LoadImage(data, true))
        {
            UpdateTexture(texture);
        }
        else
        {
            UpdateTexture(defaultImage);
        }

        if (onImageLoaded != null)
        {
            onImageLoaded(this);
        }
    }

    private void UpdateTexture(Texture tex)
    {
        Initialize();

        m_rawImage.texture = tex;
        m_rawImage.enabled = tex != null;
        if (tex)
        {
            if (m_fitTarget)
            {
                m_rawImage.FitWithin(m_fitTarget);
            }
            else if (m_aspectRatioFitter)
            {
                m_aspectRatioFitter.aspectRatio = (float)tex.width / tex.height;
            }
        }
    }

    public void CancelDownload()
    {
        if (m_webRequest != null)
        {
            m_webRequest.Abort();
            m_webRequest = null;
        }
    }
}
