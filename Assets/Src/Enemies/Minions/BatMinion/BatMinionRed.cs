using Entropek.Combat;
using Entropek.Projectiles;
using UnityEngine;

public class BatMinionRed : BatMinion
{
    private const string ShootAnimationName = "SA_Bat_Bite";
    private const string ShootAnimationEvent = "Shoot";
    private const string ShootActionAgentOutome = "Shoot";

    private const int HitScanShotId = 0;

    [Header(nameof(BatMinionRed)+" Components")]
    [SerializeField] HitScanner hitScanner;

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

    public override void Shoot(Transform target)
    {
        hitScanner.FireAt(target.position, HitScanShotId);
    }
}