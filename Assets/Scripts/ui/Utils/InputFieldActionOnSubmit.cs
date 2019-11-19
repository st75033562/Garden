using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class InputFieldActionOnSubmit : MonoBehaviour {
    public UnityEvent onSubmit;

    [SerializeField]
    private InputField m_inputField;

    [SerializeField]
    private bool m_deselectOnSubmit = true;

    [SerializeField]
    private GameObject m_actionObject;

    private bool m_needUpdate;

	// Use this for initialization
	void Start () {
        if (!m_inputField)
        {
            m_inputField = GetComponent<InputField>();
        }

        Debug.Assert(!m_inputField || m_actionObject != m_inputField.gameObject, "Use OnSubmit of InputField");

        if (m_inputField)
        {
            m_inputField.onEndEdit.AddListener(OnEndEdit);
        }
	}

    void OnDestroy()
    {
        if (m_inputField)
        {
            m_inputField.onEndEdit.RemoveListener(OnEndEdit);
        }
    }

    private void OnEndEdit(string text)
    {
        // normally, we need to hit the submit button twice to get OnSubmit triggered
        // so instead, we use onEndEdit which will be get triggered on first hit,
        // but the problem arises when user selects another control in which case
        // onEndEdit will be triggered.
        // so to filter this event, we check if the submit button is hit
        var inputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
        if (!inputModule || !Input.GetButtonDown(inputModule.submitButton))
        {
            return;
        }

        // delay update otherwise clearing the selected object can cause error if
        // SetSelectedGameObject is being called
        m_needUpdate = true;
    }

    void LateUpdate()
    {
        if (!m_needUpdate) { return; }
        m_needUpdate = false;

        bool handled = false;
        if (m_actionObject != null)
        {
            handled = ExecuteEvents.Execute(m_actionObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        if (!handled && onSubmit != null)
        {
            handled = true;
            onSubmit.Invoke();
        }

        var selected = EventSystem.current.currentSelectedGameObject;
        if (m_deselectOnSubmit && selected == m_inputField.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void Reset()
    {
        m_inputField = GetComponent<InputField>();
        m_deselectOnSubmit = true;
        m_actionObject = null;
    }
}
