using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundBlock : BlockBehaviour
{
    private SoundMenuPlugins m_menu;

    protected override void Start()
    {
        base.Start();
        m_menu = GetComponentInChildren<SoundMenuPlugins>();
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        if (m_menu.clip != null && m_menu.clip.bundleClip != null)
        {
            m_menu.clip.bundleClip.volume = float.Parse(slotValues[0]) / 100.0f;
            CodeContext.soundManager.Play(m_menu.clip.bundleClip);
        }
        yield break;
    }
}
