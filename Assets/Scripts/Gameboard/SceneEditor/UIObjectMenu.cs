using System;
using UnityEngine;

namespace Gameboard
{
    public class UIObjectMenu : MonoBehaviour
    {
        private Action m_onDismissed;

        // open the menu and notify the old caller the dismission of the menu
        public void Open(Action onDismiss)
        {
            if (onDismiss == null)
            {
                throw new ArgumentNullException("onDismiss");
            }

            if (m_onDismissed != null)
            {
                m_onDismissed();
            }
            m_onDismissed = onDismiss;

            gameObject.SetActive(true);
        }

        public void Close()
        {
            if (m_onDismissed != null)
            {
                m_onDismissed();
                m_onDismissed = null;
            }

            gameObject.SetActive(false);
        }
    }
}
