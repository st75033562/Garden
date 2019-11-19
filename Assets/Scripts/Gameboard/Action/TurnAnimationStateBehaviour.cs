using UnityEngine;

namespace Gameboard
{
    public class TurnAnimationStateBehaviour : StateMachineBehaviour, IPlayAnimationActionHandler
    {
        public string m_loopStateName;

        public bool Handle(ObjectActionPlayAnimation action, ObjectActionPlayAnimation.Config config)
        {
            if (config.name == "zuozhuan" || config.name == "youzhuan")
            {
                return false;
            }
            else
            {
                action.queuedAnimation = config;
                action.animator.SetTrigger("end_turn");
                return true;
            }
        }

        public override void OnStateEnter(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.IsName(m_loopStateName))
            {
                animator.SetBool("is_turning", true);
                animator.GetComponent<ObjectActionPlayAnimation>().actionHandler = this;
            }
        }

        public override void OnStateExit(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.IsName(m_loopStateName))
            {
                animator.ResetTrigger("end_turn");
                animator.SetBool("is_turning", false);

                var action = animator.GetComponent<ObjectActionPlayAnimation>();
                action.actionHandler = null;
                action.RunQueuedAnimation();
            }
        }
    }
}
