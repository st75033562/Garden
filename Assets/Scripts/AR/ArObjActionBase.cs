using Gameboard;
using UnityEngine;

public class ArObjActionBase : MonoBehaviour
{
    public GameObject m_VehicleModel;

    private Vector3 m_OriginalScale;
    private ObjectActionManager m_actionManager;

    protected virtual void Awake()
    {
        m_OriginalScale = m_VehicleModel.transform.localScale;
        m_actionManager = GetComponent<ObjectActionManager>();
    }

    public void DoAction(int actionId, params string[] args)
    {
        if (m_actionManager)
        {
            m_actionManager.Execute(actionId, args);
        }
    }

    public int MarkerId
    {
        get;
        set;
    }

    public virtual void ResetStatus()
    {
        if (m_actionManager)
        {
            m_actionManager.Stop();
        }

        m_VehicleModel.transform.localPosition = Vector3.zero;
        m_VehicleModel.transform.localRotation = Quaternion.identity;
        m_VehicleModel.transform.localScale = m_OriginalScale;
    }

    public int ModelId
    {
        get;
        set;
    }

    public void SetTranslation(Vector3 offset)
    {
        m_VehicleModel.transform.localPosition = offset;
    }

    public void SetScale(Vector3 scale)
    {
        m_VehicleModel.transform.localScale = Vector3.Scale(m_OriginalScale, scale);
    }

    public void SetRotation(Vector3 rotation)
    {
        m_VehicleModel.transform.localRotation = Quaternion.Euler(rotation);
    }

    public ARSceneManager SceneManager
    {
        get;
        set;
    }
}
