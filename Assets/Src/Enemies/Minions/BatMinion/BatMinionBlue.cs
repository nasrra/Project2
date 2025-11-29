using Entropek.Combat;
using UnityEngine;

public class BatMinionBlue : BatMinion
{
    private const string ShootAnimationName = "SA_Bat_Bite";
    private const string ShootAnimationEvent = "Shoot";
    private const string ShootActionAgentOutome = "Shoot";

    [Header(nameof(BatMinionBlue)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;

    public override void Shoot(Transform target)
    {
        projectileSpawner.FireAtTarget(0, 0, target);
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
                Shoot(target);
                return true;
            default:
                return false;
        }
    }

}