using System.Collections;
using UnityEngine;

public class KeyPressedBlock : BlockBehaviour
{
    private KeyMenuPlugins m_keyMenu;

    protected override void Start()
    {
        base.Start();

        m_keyMenu = GetComponentInChildren<KeyMenuPlugins>();
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var pressed = false;
        var key = m_keyMenu.GetMenuValue();
        if (key == "key_pressed_any")
        {
            foreach (var code in KeyPressedBlockUtil.allKeys)
            {
                if (Input.GetKey(code))
                {
                    pressed = true;
                    break;
                }
            }
        }
        else
        {
            pressed = Input.GetKey(m_keyMenu.keyCode);
        }
        retValue.value = pressed.ToString();
        yield break;
    }
}
