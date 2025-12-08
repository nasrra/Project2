using System.Collections;
using Entropek;
using Entropek.Audio;
using Entropek.Projectiles;
using Entropek.Vfx;
using UnityEngine;

public class FireballProjectile : Projectile, IDeactivatable
{
    private const int FireballVfxId = 0;
    private const string LoopSfxName = "FireballProjectileLoop";
    private const string IgnitionSfxName = "FireballProjectileIgnition";

    [Header("Fireball Components")]
    [SerializeField] VfxPlayerSpawner vfxPlayerSpawner;
    [SerializeField] AudioPlayer audioPlayer;
    private VfxPlayer vfxPlayer;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (vfxPlayer != null)
        {
            vfxPlayer.transform.position = transform.position;
        }
    }

    protected override void OnHitHealth(GameObject hitGameObject, Vector3 hitPoint)
    {
        base.OnHitHealth(hitGameObject, hitPoint);
    }
    protected override void OnHitOther(GameObject hitGameObject, Vector3 hitPoint)
    {
        base.OnHitOther(hitGameObject, hitPoint);
    }

    public override void Activate()
    {
        base.Activate();
        StartCoroutine(DeferredActivate());
    }

    IEnumerator DeferredActivate()
    {
        yield return null; // wait one frame.
        audioPlayer.PlaySound(IgnitionSfxName, gameObject);
        vfxPlayer = vfxPlayerSpawner.PlayVfx(FireballVfxId, transform.position, transform.forward);
        audioPlayer.PlaySound(LoopSfxName, vfxPlayer.gameObject);
        yield break;        
    }

    public override void Deactivate()
    {
        audioPlayer.StopAllSounds(false);
        vfxPlayer?.Stop(1f);
        vfxPlayer = null;
        base.Deactivate();
    }

}