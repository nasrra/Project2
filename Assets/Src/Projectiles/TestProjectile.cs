using Entropek.Audio;
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

    protected override void OnHitHealth(GameObject hitGameObject, Vector3 hitPoint)
    {
        vfxPlayerSpawner.PlayVfx(HitVfxId, hitPoint, transform.forward);
        audioPlayer.PlaySound(HitSound, hitPoint);
        base.OnHitHealth(hitGameObject, hitPoint);
    }

    protected override void OnHitOther(GameObject hitGameObject, Vector3 hitPoint)
    {
        vfxPlayerSpawner.PlayVfx(HitVfxId, hitPoint, transform.forward);
        base.OnHitOther(hitGameObject, hitPoint);
    }
}
