using AR;
using OpenCVForUnitySample;
using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ARSceneManager : MonoBehaviour
{
    public MarkerTracker m_MarkerTracker;

    [SerializeField]
    private float m_MarkerLengthCm; // marker length in centimeter

    [SerializeField]
    private bool m_ShowDefaultModel;

    public Camera m_ARCamera;

    List<int> m_MarkerIds = new List<int>();

    class ARObjectOffsetData
    {
        public Vector3 translation;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
    }

    class ARObjectActionData
    {
        public int actionId;
        public string[] actionArguments;
    }

    List<ArObjActionBase> m_ARObjects = new List<ArObjActionBase>();
    List<int> m_HiddenMarkers = new List<int>();

    Dictionary<int, ARObjectOffsetData> m_OffsetData = new Dictionary<int, ARObjectOffsetData>();
    Dictionary<int, List<ARObjectActionData>> m_ActionData = new Dictionary<int, List<ARObjectActionData>>();

    List<int> m_LostTrackingMarkers = new List<int>();

    int m_defaultModelId;

    //用于控制AR 光密度 
    Transform tsLight = null;
    Light _light = null;

    void Awake()
    {
        m_defaultModelId = ARModelCache.Instance.DefaultModelId;
        m_MarkerTracker.OnTrackingUpdated += OnTrackingUpdated;
    }

    void OnDestroy()
    {
        if (m_MarkerTracker != null)
        {
            m_MarkerTracker.OnTrackingUpdated -= OnTrackingUpdated;
        }
    }

    void OnTrackingUpdated()
    {
        m_MarkerIds.Clear();

        // update model's transform
        foreach (var marker in m_MarkerTracker.Markers)
        {
            if (marker.IsLost)
            {
                m_LostTrackingMarkers.Add(marker.Id);
                continue;
            }

            m_MarkerIds.Add(marker.Id);

            var arObject = GetOrCreateObject(marker.Id);
            if (arObject)
            {
                arObject.gameObject.SetActive(IsObjectVisible(marker.Id));

                var worldMatrix = marker.WorldMatrix;
                ARUtils.SetTransformFromMatrix(arObject.transform, ref worldMatrix);
                DoAction(arObject);
            }
        }

        foreach (var markerId in m_LostTrackingMarkers)
        {
            var obj = GetObject(markerId);
            if (obj)
            {
                obj.gameObject.SetActive(false);
            }
        }
        m_LostTrackingMarkers.Clear();
    }

    
    private void Update()
    {
        if (tsLight == null)
            tsLight = gameObject.transform.Find("Directional Light");
        if (tsLight != null && _light == null)
        {
            _light = tsLight.gameObject.GetComponent<Light>();
        }

        //FIX 现在AR灯光密度设置为1.5f
        if (_light != null && Mathf.Abs(_light.intensity - 1.5F) > Mathf.Epsilon)
        {
            _light.intensity = 1.5f;
            //Debug.LogError("Init AR Light called");
        }
        
    }

    private ArObjActionBase GetOrCreateObject(int markerId)
    {
        var arObject = GetObject(markerId);
        if (!arObject && m_ShowDefaultModel)
        {
            arObject = CreateObject(markerId, m_defaultModelId);
        }
        return arObject;
    }

    private ArObjActionBase GetObject(int markerId)
    {
        return m_ARObjects.Find(x => x.MarkerId == markerId);
    }

    protected ArObjActionBase CreateObject(int markerId, int modelId)
    {
        var template = ARModelCache.Instance.Get(modelId);
        GameObject go = Instantiate(template);

        var obj = go.GetComponent<ArObjActionBase>();
        obj.SceneManager = this;
        obj.MarkerId = markerId;
        obj.ModelId = modelId;
        obj.ResetStatus();

        OnObjectCreated(go);

        go.transform.SetParent(SceneRoot, false);
        go.SetActive(IsObjectVisible(markerId));

        ApplyOffset(obj);
        DoAction(obj);

        m_ARObjects.Add(obj);
        return obj;
    }

    protected virtual void OnObjectCreated(GameObject go) { }

    private void ApplyOffset(ArObjActionBase obj)
    {
        ARObjectOffsetData offset;
        if (m_OffsetData.TryGetValue(obj.MarkerId, out offset))
        {
            obj.SetTranslation(offset.translation);
            obj.SetScale(offset.scale);
            obj.SetRotation(offset.rotation);
        }
    }

    private void DoAction(ArObjActionBase obj)
    {
        List<ARObjectActionData> actions;
        if (obj.gameObject.activeInHierarchy && m_ActionData.TryGetValue(obj.MarkerId, out actions))
        {
            foreach (var action in actions)
            {
                obj.DoAction(action.actionId, action.actionArguments);
            }
            m_ActionData.Remove(obj.MarkerId);
        }
    }

    public void DoAction(int markerId, int actionId, params string[] args)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        for (int i = 0; i < m_ARObjects.Count; ++i)
        {
            if (markerId == m_ARObjects[i].MarkerId)
            {
                if (m_ARObjects[i].gameObject.activeInHierarchy)
                {
                    m_ARObjects[i].DoAction(actionId, args);
                }
                return;
            }
        }

        // cache the action request
        List<ARObjectActionData> actions;
        if (!m_ActionData.TryGetValue(markerId, out actions))
        {
            actions = new List<ARObjectActionData>();
            m_ActionData.Add(markerId, actions);
        }
        actions.Add(new ARObjectActionData {
            actionId = actionId,
            actionArguments = args
        });
    }

    public void SetModel(int markerId, int modelId)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        if (!IsValidModel(modelId))
        {
            Debug.Log("invalid model id: " + modelId);
            return;
        }

        var arObject = m_ARObjects.Find(x => x.MarkerId == markerId);
        if (arObject && modelId == arObject.ModelId)
        {
            return;
        }

        if (arObject)
        {
            InternalRemoveObject(markerId, false);
        }

        CreateObject(markerId, modelId);
    }

    public ArObjActionBase GetMarkerObject(int markerId)
    {
        return m_ARObjects.Find(x => x.MarkerId == markerId);
    }

    public virtual int GetMarkerObjectId(int markerId)
    {
        return 0;
    }

    bool IsValidModel(int modelId)
    {
        return ARModelCache.Instance.Get(modelId) != null;
    }

    public List<int> GetMarkerIds()
    {
        return m_MarkerIds;
    }

    public void ShowMarkerObject(int markerId)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        m_HiddenMarkers.Remove(markerId);

        var obj = GetObject(markerId);
        if (obj)
        {
            obj.gameObject.SetActive(true);
        }
    }

    public void HideMarkerObject(int markerId)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        if (!m_HiddenMarkers.Contains(markerId))
        {
            m_HiddenMarkers.Add(markerId);
        }

        var obj = GetObject(markerId);
        if (obj)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private bool IsObjectVisible(int markerId)
    {
        return !m_HiddenMarkers.Contains(markerId);
    }

    public void SetObjectRotation(int markerId, Vector3 rotation)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        GetOffsetData(markerId).rotation = rotation;
        var obj = GetObject(markerId);
        if (obj)
        {
            obj.SetRotation(rotation);
        }
    }

    public void SetObjectOffset(int markerId, Vector3 offset)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        var worldOffset = offset * WorldUnitPerCm;
        GetOffsetData(markerId).translation = worldOffset;
        var obj = GetObject(markerId);
        if (obj)
        {
            obj.SetTranslation(worldOffset);
        }
    }

    public void SetObjectScale(int markerId, Vector3 scale)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        GetOffsetData(markerId).scale = scale;
        var obj = GetObject(markerId);
        if (obj)
        {
            obj.SetScale(scale);
        }
    }

    private ARObjectOffsetData GetOffsetData(int markerId)
    {
        ARObjectOffsetData offset;
        if (!m_OffsetData.TryGetValue(markerId, out offset))
        {
            offset = new ARObjectOffsetData();
            m_OffsetData.Add(markerId, offset);
        }
        return offset;
    }

    public float WorldUnitPerCm
    {
        get { return m_MarkerTracker.MarkerLength / m_MarkerLengthCm; }
    }

    /// <summary>
    /// The scene root for all AR objects
    /// </summary>
    public Transform SceneRoot
    {
        get { return transform; }
    }

    public bool RenderingOn
    {
        get { return m_ARCamera.enabled; }
        set { m_ARCamera.enabled = value; }
    }

    public Camera ARCamera
    {
        get { return m_ARCamera; }
    }

    public void ActivateSceneObjects(bool active)
    {
        SceneRoot.gameObject.SetActive(active);
    }

    public void RemoveObjects()
    {
        foreach (var obj in m_ARObjects)
        {
            Destroy(obj.gameObject);
        }
        m_ARObjects.Clear();
        m_OffsetData.Clear();
        m_ActionData.Clear();
    }

    public void RemoveObject(int markerId)
    {
        if (markerId < 0)
        {
            throw new ArgumentOutOfRangeException("markerId");
        }

        InternalRemoveObject(markerId, true);
    }

    private void InternalRemoveObject(int markerId, bool removeOffset)
    {
        var index = m_ARObjects.FindIndex(x => x.MarkerId == markerId);
        if (index != -1)
        {
            Destroy(m_ARObjects[index].gameObject);
            m_ARObjects.RemoveAt(index);
        }
        if (removeOffset)
        {
            m_OffsetData.Remove(markerId);
        }
        m_ActionData.Remove(markerId);
    }
}
