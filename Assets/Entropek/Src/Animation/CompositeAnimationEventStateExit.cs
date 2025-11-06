using UnityEngine;

namespace Entropek.UnityUtils.AnimatorUtils
{

    public class CompositeAnimationEventStateExit : StateMachineBehaviour
    {
        private AnimationEventReciever reciever;

        [Tooltip("The event triggered first is the first entry, while the last being the last entry in this array.")]
        [SerializeField] private string[] eventNames;
        public string[] EventNames => eventNames;

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

            for(int i = 0; i < eventNames.Length; i++)
            {
                reciever.TriggeredAnimationEvent(eventNames[i]);
            }
        }
    }
    
}

