using System;
using Entropek.Combat;
using Entropek.Projectiles;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class BatMinionRed : BatMinion
{
    private const string ShootAnimationEvent = "Shoot";
    private const string ShootAnimationName = "SA_Bat_Bite";
    private const string ShootActionAgentOutome = "Shoot";

    private const string StartShotTargetingAnimationEvent = "StartShotTargeting";
    private const string StopShotTargetingAnimationEvent = "StopShotTargeting";

    private const int HitScanShotId = 0;

    [Header(nameof(BatMinionRed)+" Components")]
    
    [SerializeField] HitScanner hitScanner;
    [SerializeField] LineRendererController lineRendererController;
    
    [RuntimeField] Vector3 shotTargetPosition;

    Action ShotTargetingState;


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ShotTargetingState?.Invoke();
    }

    protected override void OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutome:
                animator.Play(ShootAnimationName);
                break;
            default:
                break;
        }
    }

    protected override bool OnAnimationEventTriggered(string eventName)
    {
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }

        switch (eventName)
        {
            case ShootAnimationEvent:
                Shoot(shotTargetPosition);
                return true;
            case StartShotTargetingAnimationEvent:
                OnStartShotTargetingAnimationEvent();                
                return true;
            case StopShotTargetingAnimationEvent:
                OnStopShotTargetingAnimationEvent();
                return true;
            default:
                return false;
        }
    }

    private void OnStartShotTargetingAnimationEvent()
    {           
        // slowly lerp in the line renderer.

        lineRendererController.LerpColorAlpha(0.025f, 0.025f, 0.334f);
        
        // snap the end point to the target immeditely.

        shotTargetPosition = target.position;
        lineRendererController.LineRenderer.SetPosition(0, lineRendererController.transform.position);
        lineRendererController.LineRenderer.SetPosition(1, shotTargetPosition);
        
        // update the shot targeting.

        ShotTargetingState = UpdateShotTargeting; 
    }

    private void UpdateShotTargeting()
    {
        shotTargetPosition = target.position;
        lineRendererController.LineRenderer.SetPosition(0, lineRendererController.transform.position);
        lineRendererController.LineRenderer.SetPosition(1, shotTargetPosition);
    }

    private void OnStopShotTargetingAnimationEvent()
    {
        ShotTargetingState = null;
    }

    public override void Shoot(Vector3 position)
    {
        // quickly fade in.

        lineRendererController.LerpColorAlpha(
            1, 1, 0.0835f, 
            ()=>
            {
                // quickly fade out afterwards.

                lineRendererController.LerpColorAlpha(
                    0,0,0.334f
                );
            }
        );

        hitScanner.FireAt(position, HitScanShotId);
    }
}