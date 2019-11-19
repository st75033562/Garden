using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class SaveCodeCapture : MonoBehaviour
{
	//public Image m_Image;
	public RectTransform m_TargetUI;
	public GameObject m_Canvas;
    public RectTransform m_Root;

    // the width of the screen shot in pixels
    public int screenshotWidth;

	GameObject m_CurObj;
	string m_SaveName;

    public static SaveCodeCapture g_Instance { get; private set; }

	void Awake()
	{
        GetComponent<Camera>().enabled = false;

		if(null == g_Instance)
		{
			g_Instance = this;
        }
	}

	public IEnumerator CaptureScreenShot(string name)
	{
        StopAllCoroutines();
        if (m_CurObj)
        {
            Destroy(m_CurObj);
        }

        var camera = GetComponent<Camera>();
        camera.enabled = true;
        m_Root.sizeDelta = (m_TargetUI.root as RectTransform).sizeDelta;
        // fit the canvas
        m_Root.localScale = Vector3.one * (camera.orthographicSize * 2) / m_Root.rect.height;

		m_SaveName = name;
		m_CurObj = Instantiate(m_TargetUI.gameObject, m_Canvas.transform);
		DestroyImmediate(m_CurObj.GetComponentInChildren<CodePanelManager>());

		RectTransform mTargetRect = m_CurObj.GetComponent<RectTransform>();

        // find the target rect size
        float width = mTargetRect.TransformVector(new Vector3(mTargetRect.rect.width, 0, 0)).magnitude;
        float height = mTargetRect.TransformVector(new Vector3(0, mTargetRect.rect.height, 0)).magnitude;

        // keep the aspect ratio
        var screenshotHeight = (int)(screenshotWidth / (width / height));

        int texHeight = (int)(camera.orthographicSize * 2 * screenshotWidth / width);
        int texWidth = (int)(texHeight * camera.aspect);

        if (camera.targetTexture)
        {
            var curTexture = camera.targetTexture;
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(curTexture);
        }

        RenderTexture renderTex = RenderTexture.GetTemporary(texWidth, texHeight, 0);
        camera.targetTexture = renderTex;

        yield return StartCoroutine(Capture(screenshotHeight));
	}

    IEnumerator Capture(int screenshotHeight)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var camera = GetComponent<Camera>();
        camera.enabled = false;

        RenderTexture.active = camera.targetTexture;
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(camera.targetTexture.width - screenshotWidth, 
                                       camera.targetTexture.height - screenshotHeight, 
                                       screenshotWidth, screenshotHeight), 
                              0, 0);
        camera.targetTexture = null;

        CodeProjectRepository.instance.saveImage(m_SaveName, screenshot.EncodeToPNG());

        RenderTexture.ReleaseTemporary(RenderTexture.active);
        RenderTexture.active = null;
        Destroy(screenshot);
        Destroy(m_CurObj);
    }

    [ContextMenu("Capture")]
    void TestCapture()
    {
        CaptureScreenShot("test");
    }
}
