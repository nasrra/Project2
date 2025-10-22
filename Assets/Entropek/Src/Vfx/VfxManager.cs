using System.Collections.Generic;
using Entropek.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Entropek.Vfx
{

    public class VfxManager : MonoBehaviour
    {

        // the Singleton object that holds all vfx containers of every vfx player.

        private static GameObject vfxPooler;

        [Header("Components")]
        [SerializeField] GameObject[] vfxPrefabs;
        
        // The container or parent object that holds all visual effect instances of this VfxPlayer instance.
        
        private GameObject vfxPool;

        // Item 1 is the actual instance of a visual effect on a gameobject.
        // Item 2 in the tuple is the id of the vfx in the visualEffects array.

        private SwapbackList<(VfxPlayer, int)> activeVfxPlayers = new();
        private Stack<VfxPlayer>[] inactiveVfxPlayers;

        private void Awake()
        {
            CreateVfxPool();
            InitialiseInactiveList();
        }

        private void LateUpdate()
        {
            ReturnCompletedVfxToPool();
        }

        private void OnDestroy()
        {
            // destroy the vfx container and all vfx within it.

            Destroy(vfxPool);
        }

        private void CreateVfxPool()
        {
            vfxPool = new() { name = gameObject.name };

            // Child to the singleton parent container.

            vfxPool.transform.SetParent(vfxPooler.transform);
        }

        private void InitialiseInactiveList()
        {
            inactiveVfxPlayers = new Stack<VfxPlayer>[vfxPrefabs.Length];
            for (int i = 0; i < inactiveVfxPlayers.Length; i++)
            {
                inactiveVfxPlayers[i] = new Stack<VfxPlayer>();
            }

        }

        private void ReturnCompletedVfxToPool()
        {

            for (int i = 0; i < activeVfxPlayers.Count; i++)
            {

                (VfxPlayer, int) vfxTuple = activeVfxPlayers[i];

                if (vfxTuple.Item1.IsFinished() == true)
                {
                    // remove the vfx tuple from the active list.

                    activeVfxPlayers.Remove(vfxTuple);

                    // push the instance onto the inactive stack to be rused later.

                    inactiveVfxPlayers[vfxTuple.Item2].Push(vfxTuple.Item1);

                    // disable any update calls for the inactive visual effect.

                    vfxTuple.Item1.gameObject.SetActive(false);
                }
            }
        }

        public void PlayVfx(int vfxId, Vector3 worldSpacePosition, Vector3 rotationEuler)
        {
            if (inactiveVfxPlayers[vfxId].TryPop(out VfxPlayer vfx))
            {
                // reuse an inactive vfx.

                vfx.gameObject.SetActive(true);
                vfx.Play(worldSpacePosition, rotationEuler);

                // push onto active list.

                activeVfxPlayers.Add(new(vfx, vfxId));
            }


            else
            {
                GameObject instanceObject = Instantiate(vfxPrefabs[vfxId]);
                VfxPlayer vfxPlayer = instanceObject.GetComponent<VfxPlayer>();
                vfxPlayer.transform.SetParent(vfxPool.transform);
                vfxPlayer.Play(worldSpacePosition, rotationEuler);
                activeVfxPlayers.Add((vfxPlayer, vfxId));
            }            
        }

        public void PlayVfx(int vfxId, Vector3 worldSpacePosition)
        {
            PlayVfx(vfxId, worldSpacePosition, Vector3.zero);
        }

        static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            internal static void Initialise()
            {
                // Singleton object creation.

                vfxPooler = new()
                {
                    name = "VfxPooler"
                };

                DontDestroyOnLoad(vfxPooler);

            }
        }
    }

}

