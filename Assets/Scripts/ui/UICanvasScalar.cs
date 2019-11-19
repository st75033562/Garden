using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// the canvas scaler which respects the viewport rect of the render camera
/// </summary>
public class UICanvasScalar : CanvasScaler
{
    // The log base doesn't have any influence on the results whatsoever, as long as the same base is used everywhere.
    private const float kLogBase = 2;

    protected Canvas m_canvas;

    protected override void OnEnable()
    {
        m_canvas = GetComponent<Canvas>();
        base.OnEnable();
    }

    protected override void HandleScaleWithScreenSize()
    {
        Vector2 screenSize;
        if (m_canvas.renderMode == RenderMode.ScreenSpaceCamera && m_canvas.worldCamera)
        {
            screenSize = m_canvas.worldCamera.pixelRect.size;
        }
        else
        {
            screenSize = new Vector2(Screen.width, Screen.height);
        }

        // Removed multiple display support until it supports none native resolutions(case 741751)
        //if (Screen.fullScreen && m_Canvas.targetDisplay < Display.displays.Length )
        //{
        //    Display disp = Display.displays[m_Canvas.targetDisplay];
        //    screenSize = new Vector2 (disp.renderingWidth, disp.renderingHeight);
        //}

        float scaleFactor = 0;
        switch (m_ScreenMatchMode)
        {
        case ScreenMatchMode.MatchWidthOrHeight:
            {
                // We take the log of the relative width and height before taking the average.
                // Then we transform it back in the original space.
                // the reason to transform in and out of logarithmic space is to have better behavior.
                // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                // In normal space the average would be (0.5 + 2) / 2 = 1.25
                // In logarithmic space the average is (-1 + 1) / 2 = 0
                float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
                float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
                float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                break;
            }
        case ScreenMatchMode.Expand:
            {
                scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                break;
            }
        case ScreenMatchMode.Shrink:
            {
                scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                break;
            }
        }

        SetScaleFactor(scaleFactor);
        SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
    }
}
