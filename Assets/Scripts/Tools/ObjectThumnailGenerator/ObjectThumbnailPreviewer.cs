#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectThumbnailPreviewer : MonoBehaviour
{
    public LayoutElement m_viewportUpPart;
    public LayoutElement m_viewportBottomPart;
    public LayoutElement m_viewportLeftPart;
    public LayoutElement m_viewportRightPart;
    public RectTransform m_objectScreenRectTrans;
    public RectTransform m_screenshotCanvasRoot;

    // for adjusting output area
    [SerializeField]
    private Vector2 m_viewportSize;

    [SerializeField]
    private Vector2 m_objectScreenPos;

    [SerializeField]
    private Vector2 m_objectScreenSize;

    [SerializeField]
    private Vector3 m_cameraPosBias;

    [SerializeField]
    private Vector2 m_uiObjectPadding;

    [SerializeField]
    private bool m_adjustUIObjectViewport;

    private GameObject m_currentTarget;

    public Vector2 viewportSize
    {
        get { return m_viewportSize; }
        set
        {
            if (m_viewportSize != value)
            {
                UpdateViewport(value);
            }
        }
    }

    public Vector2 objectScreenPos
    {
        get { return m_objectScreenPos; }
        set { m_objectScreenPos = value; }
    }

    public Vector2 objectScreenSize
    {
        get { return m_objectScreenSize; }
        set { m_objectScreenSize = value; }
    }

    public Vector3 cameraPosBias
    {
        get { return m_cameraPosBias; }
        set { m_cameraPosBias = value; }
    }

    public Vector2 uiObjectPadding
    {
        get { return m_uiObjectPadding; }
        set { m_uiObjectPadding = value; }
    }

    public bool adjustUIObjectViewport
    {
        get { return m_adjustUIObjectViewport; }
        set { m_adjustUIObjectViewport = value; }
    }

    public bool objectScreenRectVisible
    {
        get { return m_objectScreenRectTrans.gameObject.activeSelf; }
        set { m_objectScreenRectTrans.gameObject.SetActive(value); }
    }

    public int viewportWidth { get { return (int)m_viewportSize.x; } }

    public int viewportHeight { get { return (int)m_viewportSize.y; } }

    private void UpdateViewport(Vector2 size)
    {
        var canvasSize = GetComponentInParent<CanvasScaler>().referenceResolution;
        m_viewportSize = Vector2.Min(canvasSize, size);

        m_viewportLeftPart.preferredWidth = m_viewportRightPart.preferredWidth = (canvasSize.x - m_viewportSize.x) / 2;
        m_viewportUpPart.preferredHeight = m_viewportBottomPart.preferredHeight = (canvasSize.y - m_viewportSize.y) / 2;
    }

    private void UpdateObjectScreenRect()
    {
        m_objectScreenRectTrans.anchoredPosition = m_objectScreenPos;
        m_objectScreenRectTrans.SetSize(m_objectScreenSize);       
    }

    public void Focus(GameObject target)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }

        if (m_objectScreenSize.x == 0 || m_objectScreenSize.y == 0)
        {
            UnityEngine.Debug.LogError("screen rect size not valid");
            return;
        }

        if (!PrefabUtility.GetPrefabParent(target))
        {
            return;
        }

        m_currentTarget = target;

        var bounds = GetBoundsInCameraSpace(target, Quaternion.Inverse(Camera.main.transform.rotation));
        if (bounds.size != Vector3.zero)
        {
            FitTarget(bounds, m_objectScreenPos, m_objectScreenSize);
        }
        else
        {
            if (Is2DObject(target))
            {
                FocusUIObject(target);
            }
            else
            {
                var center = Quaternion.Inverse(Camera.main.transform.rotation) * target.transform.position;
                FitTarget(new Bounds(center, Vector3.one), m_objectScreenPos, m_objectScreenSize);
            }
        }
    }

    private void FocusUIObject(GameObject target)
    {
        var canvas = target.GetComponentInChildren<Canvas>();
        if (!canvas)
        {
            var rectTrans = (RectTransform)target.transform;
            rectTrans.SetParent(m_screenshotCanvasRoot, false);
            rectTrans.localPosition = Vector2.Scale(rectTrans.pivot - Vector2.one * 0.5f, rectTrans.sizeDelta);

            if (m_adjustUIObjectViewport)
            {
                viewportSize = rectTrans.sizeDelta + m_uiObjectPadding;
            }
        }
        else
        {
            var camera = Camera.main;

            target.transform.rotation = camera.transform.rotation;
            var localBounds = Utils.ComputeLocalBounds(canvas);

            // calculate the bounds in camera space
            var matToCamera = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(camera.transform.rotation), Vector3.one);
            matToCamera *= canvas.transform.localToWorldMatrix;

            var center = matToCamera.MultiplyPoint(localBounds.center);
            var size = matToCamera.MultiplyVector(localBounds.size);
            FitTarget(new Bounds(center, size), Vector2.zero, localBounds.size);

            // update the viewport to match the ui
            if (m_adjustUIObjectViewport)
            {
                viewportSize = localBounds.size.xy() + m_uiObjectPadding;
            }
        }
    }

    private void FitTarget(Bounds bounds, Vector2 objectScreenPos, Vector2 objectScreenSize)
    {
        var camera = Camera.main;
        float windowWidth = camera.nearClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2) * 2 * camera.aspect;
        var unitsPerPixel = windowWidth / camera.pixelWidth;

        // fit the target within in the screen rect
        float targetBoundAspect = bounds.size.x / bounds.size.y;
        float screenRectAspect = objectScreenSize.x / objectScreenSize.y;
        // depth of the target in the camera space
        float depth;
        if (targetBoundAspect < screenRectAspect)
        {
            // fit height
            float targetHeight = objectScreenSize.y * unitsPerPixel;
            depth = camera.nearClipPlane * bounds.size.y / targetHeight;
        }
        else
        {
            // fit width
            float targetWidth = objectScreenSize.x * unitsPerPixel;
            depth = camera.nearClipPlane * bounds.size.x / targetWidth;
        }

        float offsetX = objectScreenPos.x * unitsPerPixel * depth / camera.nearClipPlane;
        float offsetY = objectScreenPos.y * unitsPerPixel * depth / camera.nearClipPlane;

        Vector3 cameraPos;
        cameraPos.x = bounds.center.x + bounds.size.x * m_cameraPosBias.x - offsetX;
        cameraPos.y = bounds.center.y + bounds.size.y * m_cameraPosBias.y - offsetY;
        cameraPos.z = bounds.min.z + bounds.size.z * m_cameraPosBias.z - depth;

        camera.transform.position = camera.transform.rotation * cameraPos;
    }

    private Bounds GetBoundsInCameraSpace(GameObject go, Quaternion invCamRotation)
    {
        var worldToCam = Matrix4x4.TRS(Vector3.zero, invCamRotation, Vector3.one);

        bool valid = false;
        var bounds = new Bounds();
        
        foreach (var mesh in go.GetComponentsInChildren<MeshFilter>())
        {
            var m = worldToCam * mesh.transform.localToWorldMatrix;
            var b = MathUtils.Transform(mesh.sharedMesh.bounds, ref m);
            if (!valid)
            {
                bounds = b;
                valid = true;
            }
            else
            {
                bounds.Encapsulate(b);
            }
        }

        foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = worldToCam * (renderer.rootBone ? renderer.rootBone : renderer.transform).localToWorldMatrix;
            var b = MathUtils.Transform(renderer.localBounds, ref m);
            if (!valid)
            {
                bounds = b;
                valid = true;
            }
            else
            {
                bounds.Encapsulate(b);
            }
        }

        return bounds;
    }

    public static bool Is2DObject(GameObject go)
    {
        return go && go.GetComponentInChildren<CanvasRenderer>();
    }

    void OnValidate()
    {
        UpdateViewport(m_viewportSize);
        UpdateObjectScreenRect();

        if (m_currentTarget)
        {
            Focus(m_currentTarget);
        }
    }
}

#endif