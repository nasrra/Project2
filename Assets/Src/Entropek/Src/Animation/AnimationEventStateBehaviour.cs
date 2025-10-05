using UnityEngine;
using UnityEngine.Events;

namespace Entropek.UnityUtils.AnimatorUtils{


public class AnimationEventStateBehaviour : StateMachineBehaviour{
    private AnimationEventReciever reciever;
    [SerializeField] private string eventName;
    public string EventName => eventName;

    [SerializeField][Range(0f,1f)] private float triggerTime;
    public float TriggerTime => triggerTime;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
    
        float currentTime = stateInfo.normalizedTime % 1f; // wrap around when looping.
    
        if(currentTime >= triggerTime){
            NotifyEventReciever(animator);
        }
    }

    private void NotifyEventReciever(Animator animtor){
        if(reciever==null){
            reciever = animtor.GetComponent<AnimationEventReciever>();
        }
        reciever.OnAnimationEventTriggered(eventName);
    }

}


}

