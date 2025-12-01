using Entropek.Combat;
using UnityEngine;

public class BatMinionBlue : BatMinion
{
    private const string ShootAnimationName = "SA_Bat_Bite";
    private const string ShootAnimationEvent = "Shoot";
    private const string ShootActionAgentOutcome = "Shoot";

    [Header(nameof(BatMinionBlue)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;


    public override void Shoot(Vector3 position)
    {
        projectileSpawner.FireAtPosition(0, 0, position);
    }


    /// 
    /// Action Agent Outcomes.
    /// 


    protected override bool OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutcome:
                OnShootActionAgentOutcome();
                return true;
            default:
                return false;
        }
    }

    private void OnShootActionAgentOutcome()
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
                OnShootAnimationEvent();
                return true;
            default:
                return false;
        }
    }

    private void OnShootAnimationEvent()
    {
        Shoot(target.position);
    }

}