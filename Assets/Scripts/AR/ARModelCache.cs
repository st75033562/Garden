using AssetBundles;
using DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ARModelCache : MonoBehaviour
{
    public event Action<float> onInitProgressChanged;

    private Dictionary<int, GameObject> m_Templates = new Dictionary<int, GameObject>();

    private static ARModelCache s_instance;

    public static ARModelCache Instance
    {
        get;
        private set;
    }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public Coroutine Init(bool async = true)
    {
        return StartCoroutine(InitImpl(async));
    }

    IEnumerator InitImpl(bool async)
    {
        ReportProgress(0.0f);

        int i = 0;
        foreach (var objData in ARObjectDataSource.allObjects)
        {
            var request = AssetBundleManager.LoadAssetAsync(objData.bundleName, objData.assetName, typeof(GameObject));
            yield return request;

            if (string.IsNullOrEmpty(request.error))
            {
                var arObj = request.GetAsset<GameObject>().GetComponent<ArObjActionBase>();
                AddVehicleTemplate(objData.id, arObj);
            }
            else
            {
                Debug.LogError(request.error);
            }
            if (async)
            {
                ReportProgress(++i / ARObjectDataSource.objectCount);
            }
        }
        ReportProgress(1.0f);
    }

	void AddVehicleTemplate(int modelId, ArObjActionBase vehicle)
	{
		m_Templates.Add(modelId, vehicle.gameObject);
        if (DefaultModelId == 0)
        {
            DefaultModelId = modelId;
        }
    }

    private void ReportProgress(float progress)
    {
        if (onInitProgressChanged != null)
        {
            onInitProgressChanged(progress);
        }
    }

    public int DefaultModelId { get; private set; }

    public GameObject Get(int modelId)
    {
        GameObject go;
        m_Templates.TryGetValue(modelId, out go);
        return go;
    }
}
