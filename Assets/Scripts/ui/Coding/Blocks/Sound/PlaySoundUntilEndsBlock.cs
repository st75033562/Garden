using System.Collections;
using System.Collections.Generic;

public class PlaySoundUntilEndsBlock : BlockBehaviour
{
    private SoundMenuPlugins m_menu;

    protected override void Start()
    {
        base.Start();
        m_menu = GetComponentInChildren<SoundMenuPlugins>(!NodeTemplateCache.Instance.ShowBlockUI);
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        if (m_menu.clip != null && m_menu.clip.bundleClip != null)
        {
            var slotValues = new List<string>();
            yield return Node.GetSlotValues(context, slotValues);
            m_menu.clip.bundleClip.volume = float.Parse(slotValues[0]) / 100.0f;
            var sound = CodeContext.soundManager.Create(m_menu.clip.bundleClip);
            sound.Play();
            while (sound.isPlaying)
            {
                yield return null;
            }
        }
    }
}
