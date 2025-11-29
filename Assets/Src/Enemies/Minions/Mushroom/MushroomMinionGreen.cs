using Entropek.Projectiles;
using Entropek.Time;
using UnityEngine;

public class MushroomMinionGreen : MushroomMinion
{

    private const string ShootActionAgentOutcome = "Shoot";

    private const int FireballProjectileId = 0;

    [Header(nameof(MushroomMinionGreen)+"Components")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private OneShotTimer shootDelayTimer;

    private Projectile currentProjectile;
    private Transform shootTarget;


    public void Shoot(Transform target)
    {
        currentProjectile = projectilePool.ActivateFromPool(FireballProjectileId);
        
        // stop the projectile from moving.
        
        currentProjectile.Pause();
        
        // add the current projectile 
        
        currentProjectile.transform.parent = transform;
        
        currentProjectile.transform.position = shootPoint.transform.position;

        shootTarget = target;

        shootDelayTimer.Begin();
    }


    protected override void OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutcome:
                OnShootActionAgentOutome();
                break;
            default:
                Debug.LogError($"Mushroom Minion does not implement action: {actionName}");
                break;
        }
    }

    private void OnShootActionAgentOutome()
    {
        Shoot(target);
        combatAgent.BeginEvaluationLoop();
        combatAgent.BeginChosenActionCooldown();
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
        currentProjectile.Resume();
    }
}