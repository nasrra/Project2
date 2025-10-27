using UnityEngine;

namespace Entropek.UnityUtils.AnimatorUtils
{

    public class AnimationEventStateExitBehaviour : StateMachineBehaviour
    {
        private AnimationEventReciever reciever;
        [SerializeField] private string eventName;
        public string EventName => eventName;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            NotifyEventReciever(animator);
        }

        private void NotifyEventReciever(Animator animtor)
        {
            if (reciever == null)
            {
                reciever = animtor.GetComponent<AnimationEventReciever>();
            }
            reciever.TriggeredAnimationEvent(eventName);
        }
    }
    
}

