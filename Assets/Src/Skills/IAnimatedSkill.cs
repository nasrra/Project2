using System;
using System.Collections;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;


public interface IAnimatedSkill
{
    public const string CoyoteStateEventName = "EnterAnimatedSkillCoyoteState";
    public const int MaxAnimationLayerWeight = 1;
    public const int MinAnimationLayerWeight = 0;

    Skill Skill{get;}
    Player Player{get;}
    Action AnimationCompleted {get; set;}
    Animator Animator {get;}
    AnimationEventReciever AnimationEventReciever {get;}
    string AnimationName{get;}
    string AnimationCompletedEventName{get;}
    int AnimationLayer{get;}
    Coroutine AnimationLayerWeightTransitionCoroutine{get; protected set;}
    bool AnimationCancel{get;}


    bool CanUseAnimatedSkill()
    {
        if (AnimationCancel == false)
        {
            // only use an animated skill when another animated skill isnt currently in use.
            // we dont allow animation cancelling for skills.

            return !Player.SkillCollection.AnimatedSkillIsInUse(out _);
        }

        return true;
    }

    /// <summary>
    /// Immeditely plays the assigned animation via AnimationName.
    /// </summary>

    void PlayAnimation()
    {
        // rebind, so the transform of the skinned mesh is correctly reset to its 
        // "rest" position before abruptly switching animations.
        // Otherwise animations will retain their transform offsets from one animation to another
        // if they are not modified by the new animation.

        // Animator.Rebind();

        // force animation to play with 0 normalisedTime, 
        // ensuring to override an animation that may have been queued
        // during a coyote state switch (For Player).

        Animator.Play(AnimationName, AnimationLayer, 0);
        Animator.Update(0); //<-- use this instead of rebind, does the same thing but doesnt completely refresh the entire animation state.
    }


    /// 
    /// Linkage.
    /// 


    void LinkAnimatedSkillEvents()
    {
        LinkAnimationEventRecieverEvents();
    }

    void UnlinkAnimatedSkillEvents()
    {
        UnlinkAnimationEventRecieverEvents();
    }

    protected void LinkAnimationEventRecieverEvents()
    {
        // NOTE:
        //  Perform the completed and coyote state checks here so the dont have to
        //  be written in every implementation. They are also separate because the 
        //  completed event name is not a compile time constant.

        AnimationEventReciever.AnimationEventTriggered += AnimationEventIsCompletedEvent;
        AnimationEventReciever.AnimationEventTriggered += AnimationEventIsCoyoteStateEvent;
        AnimationEventReciever.AnimationEventTriggered += OnAnimationEventTriggered;
    }

    protected void UnlinkAnimationEventRecieverEvents()
    {
        // NOTE:
        //  Perform the completed and coyote state checks here so the dont have to
        //  be written in every implementation. They are also separate because the 
        //  completed event name is not a compile time constant.

        AnimationEventReciever.AnimationEventTriggered -= AnimationEventIsCompletedEvent;
        AnimationEventReciever.AnimationEventTriggered -= AnimationEventIsCoyoteStateEvent;
        AnimationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggered;        
    }

    void AnimationEventIsCompletedEvent(string eventName)
    {        
        if (eventName == AnimationCompletedEventName)
        {
            OnAnimationCompleted();
            
            //  Exit Coyote state directly after exiting an action/animation state
            //  to ensure any queued actions occur directly after the action has finished.
            
            // Note:
            //  Do NOT put these exit coyote calls inside the actual functions (Skill1(), Skill2(), etc), that will cause
            //  a recursive loop as exit coyote state calls the stop functions of a given player state. 

            Player.EnterRestState();
            Player.ExitCoyoteState();
            AnimationCompleted?.Invoke();
        }
    }

    void StartAnimationLayerWeightTransition(float value, float speed)
    {
        if(AnimationLayerWeightTransitionCoroutine != null)
        {
            Skill.StopCoroutine(AnimationLayerWeightTransitionCoroutine);
        }
        AnimationLayerWeightTransitionCoroutine = Skill.StartCoroutine(AnimationLayerWeightTransition(value, speed));
    }

    IEnumerator AnimationLayerWeightTransition(float value, float speed)
    {
        float layerWeight = Animator.GetLayerWeight(AnimationLayer);
        
        while(layerWeight != value)
        {
            Animator.SetLayerWeight(AnimationLayer, Mathf.MoveTowards(layerWeight, value, Time.deltaTime * speed));
            layerWeight = Animator.GetLayerWeight(AnimationLayer);
            yield return new WaitForFixedUpdate();
        }

        yield break;
    }

    void OnAnimationCompleted();

    void Cancel();

    /// <summary>
    /// Switches the coyote state of the assigned PLayer to AnimatedSkill.
    /// </summary>
    /// <param name="eventName">The eventName of the coyote state state switch event.</param>

    void AnimationEventIsCoyoteStateEvent(string eventName)
    {
        if(eventName == CoyoteStateEventName)
        {
            Player.EnterCoyoteState(CoyoteState.AnimatedSkill);
        }
    }

    void OnAnimationEventTriggered(string eventName);
}
