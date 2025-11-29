using Entropek.Projectiles;
using Entropek.Time;
using UnityEngine;

public class MushroomMinionGreen : MushroomMinion
{

    private const string ShootActionAgentOutcome = "Shoot";

    private const int FireballProjectileId = 0;

    [Header(nameof(MushroomMinionGreen)+" Components")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private OneShotTimer shootDelayTimer;

    private Projectile currentProjectile;
    private Transform shootTarget;


    /// 
    /// Unique Functions.
    /// 

    protected override string GetFootstepSfx() => "FootstepGrassMedium";

    public void Shoot(Transform target)
    {
        currentProjectile = projectilePool.ActivateFromPool(FireballProjectileId);
        
        // stop the projectile from moving.
        
        currentProjectile.Pause();
        
        // make the projectile follow this gameObject.

        currentProjectile.transform.parent = transform;
        currentProjectile.transform.position = shootPoint.transform.position;

        shootTarget = target;
        shootDelayTimer.Begin();
    }


    /// 
    /// Action Agent Outcomes.
    /// 


    protected override bool OnCombatActionChosen(in string actionName)
    {
        if(base.OnCombatActionChosen(actionName) == true)
        {
            return true;
        }
        switch (actionName)
        {
            case ShootActionAgentOutcome:
                OnShootActionAgentOutome();
                return true;
            default:
                return false;
        }
    }

    private void OnShootActionAgentOutome()
    {
        Shoot(target);
    }


    /// 
    /// Linkage Override.
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
        currentProjectile.transform.parent = projectilePool.PoolContainer.transform;
        currentProjectile.transform.LookAt(shootTarget.position);

        combatAgent.BeginEvaluationLoop();
        combatAgent.BeginChosenActionCooldown();

        currentProjectile.Resume();
    }
}