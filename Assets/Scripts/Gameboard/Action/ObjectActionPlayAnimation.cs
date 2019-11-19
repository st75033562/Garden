using UnityEngine;

namespace Gameboard
{
    public interface IPlayAnimationActionHandler
    {
        bool Handle(ObjectActionPlayAnimation action, ObjectActionPlayAnimation.Config config);
    }
    
    public class ObjectActionPlayAnimation : ObjectAction
    {
        public class Config
        {
            public string name;

            [FieldValue(float.MaxValue)]
            public float speed = float.MaxValue;
        }

        [SerializeField]
        public Animator m_animator;

        private int m_lastState;
        private int m_defaultState;

        void Awake()
        {
            m_defaultState = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }

        public Animator animator { get { return m_animator; } }

        public IPlayAnimationActionHandler actionHandler { get; set; }

        public Config queuedAnimation { get; set; }

        public void RunQueuedAnimation()
        {
            if (queuedAnimation != null)
            {
                var config = queuedAnimation;
                queuedAnimation = null;
                Execute(config);
            }
        }

        public override void Execute(object o, params string[] args)
        {
            var config = (Config)o;
            if (actionHandler == null || !actionHandler.Handle(this, config))
            {
                if (config.speed != float.MaxValue)
                {
                    m_animator.SetFloat("speed", config.speed);
                }
                SetState(Animator.StringToHash(config.name));
            }
            else
            {
                m_lastState = 0;
            }
        }

        private void SetState(int state)
        {
            if (m_lastState != 0)
            {
                m_animator.ResetTrigger(m_lastState);
            }
            if (state != 0)
            {
                m_animator.SetTrigger(state);
            }
            m_lastState = state;
        }

        public override void Stop()
        {
            m_lastState = 0;
            m_animator.Play(m_defaultState);

            foreach (var p in m_animator.parameters)
            {
                switch (p.type)
                {
                case AnimatorControllerParameterType.Bool:
                    m_animator.SetBool(p.nameHash, p.defaultBool);
                    break;
                case AnimatorControllerParameterType.Float:
                    m_animator.SetFloat(p.nameHash, p.defaultFloat);
                    break;
                case AnimatorControllerParameterType.Int:
                    m_animator.SetInteger(p.nameHash, p.defaultInt);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    m_animator.ResetTrigger(p.nameHash);
                    break;
                }
            }
        }

        void Reset()
        {
            m_animator = GetComponent<Animator>();
        }
    }
}
