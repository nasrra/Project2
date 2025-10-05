using System;
using UnityEngine;

namespace Entropek.UnityUtils.AnimatorUtils{


public class AnimationEventReciever : MonoBehaviour{
    public event Action<string> AnimationEventTriggered;

    public void OnAnimationEventTriggered(string eventName){
        AnimationEventTriggered?.Invoke(eventName);
    }
}


}

