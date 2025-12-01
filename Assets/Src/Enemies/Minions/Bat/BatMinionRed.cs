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

    private const float MinShotTargetingAccuracy = 8f;
    private const float MaxShotTargetingAccuracy = 10.5f;
    private const int HitScanShotId = 0;

    [Header(nameof(BatMinionRed)+" Components")]
    
    [SerializeField] HitScanner hitScanner;
    [SerializeField] LineRendererController lineRendererController;
        
    [RuntimeField] float shotTargetingAccuracy = 0;
    
    [RuntimeField] Vector3 shotTargetPosition;

    Action ShotTargetingState;


    /// 
    /// Base.
    /// 


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ShotTargetingState?.Invoke();
    }


    ///
    /// Unique Functions.
    /// 


    public override void Shoot(Vector3 position)
    {
        // quickly fade in.

        lineRendererController.LerpColorAlpha(
            1, 1, 0.0835f, 
            ()=>
            {
                // quickly fade out afterwards.

                lineRendererController.LerpColorAlpha(
                    0,0,0.167f
                );
            }
        );

        hitScanner.FireAt(position, HitScanShotId);
    }

    /// <summary>
    /// A fixed time step for tracking the current target when preparing to shoot.
    /// </summary>

    private void UpdateShotTargeting()
    {
        // lerp to the target's position based on the current shot accuray.

        shotTargetPosition = Vector3.Lerp(shotTargetPosition, target.position, shotTargetingAccuracy * Time.deltaTime);
        
        // sync the line renderer with the new shot target position.
        
        lineRendererController.LineRenderer.SetPosition(0, lineRendererController.transform.position);
        lineRendererController.LineRenderer.SetPosition(1, shotTargetPosition);
    }


    /// 
    /// Action Agent Outcomes.
    /// 


    protected override bool OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutome:
                OnShootActionAgentOutome();
                return true;
            default:
                return false;
        }
    }

    private void OnShootActionAgentOutome()
    {
        animator.Play(ShootAnimationName);
    }


    /// 
    /// Animation Events.
    /// 


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
        shotTargetingAccuracy = UnityEngine.Random.Range(MinShotTargetingAccuracy, MaxShotTargetingAccuracy);
        
        // slowly lerp in the line renderer.

        lineRendererController.LerpColorAlpha(0.025f, 0.025f, 0.334f);
        
        // snap the end point to the target immeditely.

        shotTargetPosition = target.position;
        lineRendererController.LineRenderer.SetPosition(0, lineRendererController.transform.position);
        lineRendererController.LineRenderer.SetPosition(1, shotTargetPosition);
        
        // update the shot targeting.

        ShotTargetingState = UpdateShotTargeting; 
    }


    private void OnStopShotTargetingAnimationEvent()
    {
        ShotTargetingState = null;
    }
}