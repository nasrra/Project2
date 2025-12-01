using System;
using System.IO;
using Entropek.Combat;
using Entropek.Projectiles;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using Entropek.Vfx;
using UnityEngine;

public class GolemMinion : Minion
{
    private const string StunAnimation = "SA_Golem_Damage";
    private const string AttackAnimation = "SA_Golem_Hit";

    private const string AttackAnimationEvent = "AttackFrame";
    private const string FootstepAnimationEvent = "Footstep";
    private const string FootstepSfx = "FootstepGrassHeavy";

    private const string AttackActionAgentOutcome = "Attack";
    private const string ShootActionAgentOutcome = "Shoot";

    private const float MinShotTargetingAccuracy = 8f;
    private const float MaxShotTargetingAccuracy = 10.5f;
    private const int HitScanShotId = 0;


    [Header(nameof(GolemMinion)+" Components")]
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;
    [SerializeField] HitScanner hitScanner;
    [SerializeField] LineRendererController lineRendererController;
    [SerializeField] OneShotTimer shootDelayTimer;
    [SerializeField] SingleVfxPlayer attackSmokeVfx;

    [RuntimeField] Vector3 shotTargetPosition;
    [RuntimeField] float shotTargetingAccuracy = 0;    


    private Action ShotTargetingCallback;


    /// 
    /// Base.
    /// 


    private void FixedUpdate()
    {
        FaceMoveDirection();
        ShotTargetingCallback?.Invoke();
    }


    ///
    /// State Machine. 
    ///


    public override void AttackState()
    {
        base.AttackState();
        navAgentMovement.PausePath();
    }

    public override void IdleState()
    {
    }

    public override void IdleState(float time)
    {
        IdleState();
        stateQeueue.Enqueue(aiActionAgent.BeginEvaluationLoop);
        stateQeueue.Begin(time);
    }

    public override void Kill()
    {
        Destroy(gameObject);
    }

    public void Shoot(Vector3 position)
    {
        // quickly fade in.

        lineRendererController.LerpColorAlpha(
            1,1,0.0835f,
            () =>
            {
                // quickly fade out afterwards.

                lineRendererController.LerpColorAlpha(
                    0,0,0.167f
                );
            }
        );

        hitScanner.FireAt(position, HitScanShotId);

        StopShotTargeting();

        if(aiActionAgent.ChosenAction != null
        && aiActionAgent.ChosenAction.Name == ShootActionAgentOutcome)
        {
            aiActionAgent.BeginChosenActionCooldown();
            aiActionAgent.BeginEvaluationLoop();
        }
    }

    protected override void OnHealthDeath()
    {
        Kill();
    }

    protected override void OnOpponentEngaged(Transform opponent)
    {
        target = opponent;
    }

    protected override void EnterStunStateInternal()
    {
        animator.Play(StunAnimation);
    }

    protected override void ExitStunStateInternal()
    {
        // do nothing.
    }

    private void StartShotTargeting()
    {
        ShotTargetingCallback = UpdateShotTargeting;
        shotTargetPosition = target.position;
        shotTargetingAccuracy = UnityEngine.Random.Range(MinShotTargetingAccuracy, MaxShotTargetingAccuracy);
    }

    private void StopShotTargeting()
    {
        ShotTargetingCallback = null;        
    }

    private void UpdateShotTargeting()
    {
        // lerp to the target's position based on the current shot accuray.

        shotTargetPosition = Vector3.Lerp(shotTargetPosition, target.position, shotTargetingAccuracy * Time.deltaTime);
        
        // sync the line renderer with the new shot target position.
        
        lineRendererController.LineRenderer.SetPosition(0, lineRendererController.transform.position);
        lineRendererController.LineRenderer.SetPosition(1, shotTargetPosition);
       
    }


    /// 
    /// Combat Ai Action Agent Outcomes.
    /// 


    protected override bool OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutcome:
                OnShootActionAgentOutcome();
                return true;
            case AttackActionAgentOutcome:
                OnAttackActionAgentOutcome();
                return true;
            default:
                return false;
        }
    }

    private void OnShootActionAgentOutcome()
    {
        lineRendererController.LerpColorAlpha(0.1f,0.1f,0.5f);
        shootDelayTimer.Begin();
        StartShotTargeting();
    }

    private void OnAttackActionAgentOutcome()
    {
        animator.Play(AttackAnimation);
        AttackState();
    }


    ///
    /// Animation Events.
    /// 


    protected override bool OnAnimationEventTriggered(string eventName)
    {
        if (base.OnAnimationEventTriggered(eventName))
        {
            return true;
        }
        switch (eventName)
        {
            case FootstepAnimationEvent:
                OnFootstepAnimationEvent();
                return true;
            case AttackAnimationEvent:
                OnAttackAnimationEvent();
                return true;
            default: 
                return false;
        }
    }

    private void OnFootstepAnimationEvent()
    {
        audioPlayer.PlaySound(FootstepSfx, transform.position);
    }

    private void OnAttackAnimationEvent()
    {
        attackSmokeVfx.Play();
    }


    /// 
    /// Timer event linkage.
    ///


    protected override void LinkTimerEvents()
    {
        base.LinkTimerEvents();
        shootDelayTimer.Timeout += OnShootDelayTimeout;
    }

    protected override void UnlinkTimerEvents()
    {
        base.UnlinkTimerEvents();
        shootDelayTimer.Timeout -= OnShootDelayTimeout;
    }

    private void OnShootDelayTimeout()
    {
        Shoot(shotTargetPosition);
    }

    /// 
    /// Health Linkage.
    /// 


    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        // if((damageContext.DamageType & StaggerDamageType) != 0)
        // {
        EnterStunState(0.64f);
        // }
    }

}
