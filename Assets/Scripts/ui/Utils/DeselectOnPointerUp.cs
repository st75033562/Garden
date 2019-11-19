using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// make sure this is listed before Selectable component in the editor inspector in order to 
// get OnPointerDown called first
public class DeselectOnPointerUp : MonoBehaviour, ISelectHandler, IPointerDownHandler
{
    private GameObject m_expectedSelection;
    private GameObject m_oldSelected;

    public void OnPointerDown(PointerEventData eventData)
    {
        m_oldSelected = EventSystem.current.currentSelectedGameObject;
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_expectedSelection = EventSystem.current.currentSelectedGameObject;
        StartCoroutine(RestoreSelection());
    }

    private IEnumerator RestoreSelection()
    {
        yield return new WaitForEndOfFrame();

        // HACK to restore the selection
        if (EventSystem.current.currentSelectedGameObject == m_expectedSelection)
        {
            EventSystem.current.SetSelectedGameObject(m_oldSelected);
        }
        m_expectedSelection = null;
        m_oldSelected = null;
    }

}
