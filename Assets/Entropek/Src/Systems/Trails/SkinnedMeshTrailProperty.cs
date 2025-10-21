using System;
using Entropek.Collections;
using Entropek.Time;
using UnityEngine;

namespace Entropek.Systems.Trails{


[CreateAssetMenu(menuName="Entropek/System/Trails/SkinnedMeshTrailProperty")]
public class SkinnedMeshTrailProperty : ScriptableObject{
    [Header(nameof(SkinnedMeshTrailProperty))]
    [SerializeField] private MonobehvaiourTypeReference[] scriptsToAdd;
    [SerializeField] private Material materialOverride;
    [SerializeField] private float lifetime;

    public virtual GameObject Instantiate(SkinnedMeshRenderer skinnedMesh){
        
        // create and add mesh components.
        
        GameObject gameObject = new GameObject();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        
        // bake the current state of the skinned mesh into the mesh.

        Mesh mesh = new Mesh();
        skinnedMesh.BakeMesh(mesh);
        mf.mesh = mesh;

        // apply override material if we have one.

        if(materialOverride!=null){
            mr.material = materialOverride;
        }
        else{
            mr.material = skinnedMesh.material;
        }

        // add any scripts that are needed.

        for(int i = 0; i < scriptsToAdd.Length; i++){
            gameObject.AddComponent(scriptsToAdd[i].Type);
        }

        // assign and begin the lifetime timer.

        Timer lifetimeTimer = gameObject.AddComponent<Timer>();
        lifetimeTimer.Timeout += () => Destroy(gameObject);
        lifetimeTimer.Begin(lifetime);

        return gameObject;
    }

}


}

