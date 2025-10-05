using Entropek.Systems.Autoload;
using UnityEngine;

namespace Entropek.Systems.Trails{


    public class SkinnedMeshTrailSystem : MonoBehaviour{
        [Header("Components")]

        // the rate at which to spawn meshes.
        
        [SerializeField] private Timer spawnRate;
        
        // the lifetime of the loop to spawn backed mesh instances.
        
        [SerializeField] private Timer loopTime;

        [Header("Trail Properties")]
        [SerializeField] private SkinnedMeshRenderer[] skinnedMeshes;
        [SerializeField] SkinnedMeshTrailProperty trailProperty;

        private void OnEnable(){
            LinkEvents();
        }

        private void OnDisable(){
            UnlinkEvents();
        }

        public void SpawnMeshes(){
            for(int i = 0; i < skinnedMeshes.Length; i++){
                Transform instance = trailProperty.Instantiate(skinnedMeshes[i]).transform;
                instance.position = transform.position;
                instance.rotation = transform.rotation;
            }            
        }

        private void LinkEvents(){
            LinkTimerEvents();
        }

        private void UnlinkEvents(){
            UnlinkTimerEvents();
        }

        private void LinkTimerEvents(){
            spawnRate.Timeout += OnSpawnRateTimeout;
            loopTime.Timeout += OnLoopTimeTimeout;
        }

        private void UnlinkTimerEvents(){

        }

        private void OnSpawnRateTimeout(){
            SpawnMeshes();
        }

        private void OnLoopTimeTimeout(){
            spawnRate.Halt();
        }
    }


}

