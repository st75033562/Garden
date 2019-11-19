using System;
using UnityEngine;

[RequireComponent(typeof(Animation))]
[ExecuteInEditMode]
public class SceneAnimation : SceneTransition
{
    [SerializeField]
    private AnimationClip m_animIn;

    [SerializeField]
    private AnimationClip m_animOut;

    private Animation m_anim;

    private bool m_playing;

    protected virtual void Awake()
    {
        m_anim = GetComponent<Animation>();
        m_anim.playAutomatically = false;
    }

    public override void Begin(SceneTransition.Direction d, Action<SceneTransition> done)
    {
        base.Begin(d, done);

        m_playing = true;

        if (d == Direction.In)
        {
            m_anim.Play(m_animIn.name);
        }
        else
        {
            m_anim.Play(m_animOut.name);
        }
        // force updating, otherwise screen will flicker
        m_anim.Sample();
        OnUpdate();
    }

    private void Update()
    {
        if (m_playing)
        {
            OnUpdate();
            if (!m_anim.isPlaying)
            {
                End();
            }
        }
    }

    protected virtual void OnUpdate()
    {
    }

    public override void End()
    {
        base.End();
        m_playing = false;
    }

    [ContextMenu("Setup")]
    public virtual void Setup()
    {
        if (m_animIn)
        {
            m_anim.AddClip(m_animIn, m_animIn.name);
        }
        if (m_animOut)
        {
            m_anim.AddClip(m_animOut, m_animOut.name);
        }
    }
}
