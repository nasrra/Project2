using Entropek.Audio;
using Entropek.Combat;
using Entropek.Projectiles;
using Entropek.Vfx;
using UnityEngine;

public class TestProjectile : Projectile
{
    private const string HitSound = "MeleeHit";
    private const int HitVfxId = 0;

    [Header("Test Projectile Components")]
    [SerializeField] VfxPlayerSpawner vfxPlayerSpawner;
    [SerializeField] AudioPlayer audioPlayer;

    protected override void OnHitHealth(HitboxHitContext context)
    {
        vfxPlayerSpawner.PlayVfx(HitVfxId, context.HitPoint, transform.forward);
        audioPlayer.PlaySound(HitSound, context.HitPoint);
        base.OnHitHealth(context);
    }

    protected override void OnHitOther(HitboxHitContext context)
    {
        vfxPlayerSpawner.PlayVfx(HitVfxId, context.HitPoint, transform.forward);
        base.OnHitOther(context);
    }
}
