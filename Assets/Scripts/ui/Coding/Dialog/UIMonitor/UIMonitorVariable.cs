using UnityEngine;

public class UIMonitorVariable : MonoBehaviour
{
    public RectTransform m_content;
    public GameObject m_lineTemplate;
    public GameObject m_varTemplate;

    private VariableManager m_varManager;

    public void Configure(VariableManager varManager)
    {
        m_varManager = varManager;

        foreach (var varData in m_varManager)
        {
            var instance = (GameObject)Instantiate(m_varTemplate, m_content);
            instance.SetActive(true);
            var item = instance.GetComponent<UIVarItem>();
            item.Init(varData);

            Instantiate(m_lineTemplate, m_content).gameObject.SetActive(true);
        }
    }
}