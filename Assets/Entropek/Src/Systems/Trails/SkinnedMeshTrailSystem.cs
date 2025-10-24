using UnityEngine;

namespace Entropek.Systems.Trails{


    public class SkinnedMeshTrailSystem : MonoBehaviour{
        [Header("Components")]

        [Tooltip("The rate at which to spawn meshes.")]
        [SerializeField] private Time.LoopedTimer spawnRateTimer;
                
        [Tooltip("The lifetime of the loop to spawn backed mesh instances.")]
        [SerializeField] private Time.OneShotTimer spawnLoopTimer;

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
            spawnRateTimer.Timeout += OnSpawnRateTimeout;
            spawnLoopTimer.Timeout += OnLoopTimeTimeout;
        }

        private void UnlinkTimerEvents(){

        }

        private void OnSpawnRateTimeout(){
            SpawnMeshes();
        }

        private void OnLoopTimeTimeout(){
            spawnRateTimer.Halt();
        }
    }


}

