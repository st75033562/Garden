using System.Collections;
using UnityEngine;

public class WhenKeyPressedBlock : BlockBehaviour
{
    private KeyMenuPlugins m_keyMenu;
    private MainNode m_mainNode;
    private bool m_done;

    protected override void Start()
    {
        base.Start();
		m_keyMenu = GetComponentInChildren<KeyMenuPlugins>();
        m_mainNode = GetComponent<MainNode>();

        if (CodeContext != null)
        {
            CodeContext.eventBus.AddListener(EventId.KeyPressed, OnKeyPressed);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (CodeContext != null)
        {
            CodeContext.eventBus.RemoveListener(EventId.KeyPressed, OnKeyPressed);
        }
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        m_done = false;
        while (!m_done)
        {
            yield return null;
        }
    }

    private void OnKeyPressed(object o)
    {
        // ignore event for template node
        if (Node.IsTemplate)
        {
            return;
        }

        var selectedKey = m_keyMenu.GetMenuValue();
        if (selectedKey == "key_pressed_any")
        {
            foreach (var code in KeyPressedBlockUtil.allKeys)
            {
                if (Input.GetKey(code))
                {
                    if (m_mainNode.TryStart())
                    {
                        m_done = true;
                    }
                    return;
                }
            }
        }
        else
        {
            var code = m_keyMenu.keyCode;
            if (code != KeyCode.None && Input.GetKey(code))
            {
                if (m_mainNode.TryStart())
                {
                    m_done = true;
                }
                //Debug.Log(code + " pressed");
            }
        }
    }
}
