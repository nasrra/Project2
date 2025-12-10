using System;
using System.Collections.Generic;
using Entropek.Combat;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    public event Action<HitboxHitContext> HealthHit;
    public event Action<HitboxHitContext> OtherHit;

    [Header("Components")]
    [SerializeField] private List<Hitbox> hitboxes;


    /// 
    /// Base.
    /// 


    private void Awake()
    {   
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    public void AddHitbox(Hitbox hitbox)
    {
        hitboxes.Add(hitbox);
        LinkHitbox(hitbox);
    }

    public void RemoveHitbox(Hitbox hitbox)
    {
        hitboxes.Remove(hitbox);
        UnlinkHitbox(hitbox);
    }


    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        for(int i = 0; i < hitboxes.Count; i++)
        {
            LinkHitbox(hitboxes[i]);
        }
    }

    private void UnlinkEvents()
    {
        for(int i = 0; i < hitboxes.Count; i++)
        {
            UnlinkHitbox(hitboxes[i]);
        }        
    }

    private void LinkHitbox(Hitbox hitbox)
    {
        hitbox.HitHealth += OnHitHealth;
        hitbox.HitOther += OnHitOther;
    }

    private void UnlinkHitbox(Hitbox hitbox)
    {        
        hitbox.HitHealth -= OnHitHealth;
        hitbox.HitOther -= OnHitOther;
    }

    private void OnHitHealth(HitboxHitContext context)
    {
        HealthHit?.Invoke(context); 
    }

    private void OnHitOther(HitboxHitContext context)
    {        
        OtherHit?.Invoke(context); 
    }
}
