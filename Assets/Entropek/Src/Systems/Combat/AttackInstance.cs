using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Entropek.Systems.Combat{


[Serializable]
public class AttackInstance{
    [SerializeField] private Hitbox hitbox;
    public Hitbox Hitbox => hitbox;
    [SerializeField] private VisualEffect[] vfx;
    [SerializeField] private float damage;

    public void Begin(){
        if(hitbox.Timer.HasTimedOut==false){
            return;
        }

        hitbox.Enable();
        for(int i = 0; i < vfx.Length; i++){
            vfx[i].Play();
        }
    }

    private void ApplyDamage(GameObject other){
        if(damage <= 0){
            return;
        }

        EntityStats.Health health = other.GetComponent<EntityStats.Health>();
        health.Damage(damage);
    }

    // called in Attack Manager.

    public void LinkEvents(){
        hitbox.Hit += ApplyDamage;
    }

    // called in Attack Manager.

    public void UnlinkEvents(){    
        hitbox.Hit -= ApplyDamage;
    }
}


}

