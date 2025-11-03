using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entropek.Collections
{    
    [System.Serializable]
    public abstract class GameObjectPool<T> : MonoBehaviour where T : MonoBehaviour, IDeactivatable
    {   
        [Header("Data")]
        
        protected GameObject poolContainer;
        
        [SerializeField] private GameObject[] prefabs;
        
        // Key: The active instance of monobehaviour.
        // Value: The index of the prefab in the prefabs array.
        
        protected Dictionary<T, int> active = new();
        
        // Note:
        // using hashset instead of Stack as gameobjects in a pool can be activated outside 
        // of the pooler (via Activate() in IDeactivatable).
        // The item can quickly put itself into the active dictionary and removed from inactive this way.

        protected HashSet<T>[] inactive;

        /// 
        /// Base.
        /// 

        private void Awake()
        {
            CreatePoolContainer();
            InitialiseInactiveStack();
        }

        /// <summary>
        /// Sets the stack size to that of the amount of prefabs
        /// and initialises the internal arrays for each one.
        /// </summary>

        private void InitialiseInactiveStack()
        {
            inactive = new HashSet<T>[prefabs.Length];
            for (int i = 0; i < inactive.Length; i++)
            {
                inactive[i] = new HashSet<T>();
            }

        }

        /// <summary>
        /// Activates activates a gameobject instance stored in the reuse pool.
        /// </summary>
        /// <param name="id">The id associated with the prefab to activate.</param>
        /// <param name="isNewInstance">Whether or not the instance activated was newly instantiated or not.</param>
        /// <returns>The monobehaviour script associated with this gameobject pool.</returns>

        public T ActivateFromPool(int id)
        {
            T monoBehaviour;

            #if UNITY_EDITOR
            if(inactive.Count() <= id)
            {
                throw new InvalidOperationException($"{GetType().Name} does not have an entry for requested prefab id: {id}");
            }
            #endif

            if(inactive[id].Count > 0)
            {                
                // Note:
                //  get any instance from the front of the inactive hashset list;
                //  ordering is lost because of hashing but it doesnt matter in pooling. 

                monoBehaviour = inactive[id].First();                
            }
            else
            {
                // create a new one if there are none inactive.

                GameObject instanceObject = Instantiate(prefabs[id]);

                instanceObject.transform.SetParent(poolContainer.transform);

                // Get component.

                monoBehaviour = instanceObject.GetComponent<T>();
            
                // link Deactivated so that when this object is deactivated
                monoBehaviour.Activated += () =>
                {
                    OnInstanceActivated(monoBehaviour, id);
                }; 

                monoBehaviour.Deactivated += () =>
                {
                    OnInstanceDeactivated(monoBehaviour, id);
                };
            }

            // Activate the instance.
            
            monoBehaviour.Activate();                

            // return the instance to operate on.

            return monoBehaviour;
        }

        /// <summary>
        /// Returns an active monobehvaiour instance back to the pool for later reuse.
        /// </summary>
        /// <param name="activeInstance">The monobehaviour instance being pooled.</param>
        /// <returns>true, if this pool is tracking the activeInstance; otherwise false.</returns>

        public bool ReturnToPool(T activeInstance)
        {
            if (active.ContainsKey(activeInstance) == true)
            {
                // remove the vfx tuple from the active list.
                int prefabId = active[activeInstance];
                active.Remove(activeInstance);

                // push the instance onto the inactive stack to be rused later.

                inactive[prefabId].Add(activeInstance);

                // disable any update calls for the inactive visual effect.

                activeInstance.gameObject.SetActive(false);

                return true;
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="id"></param>

        private void OnInstanceActivated(T instance, int id)
        {
            // short circuit if the instance has already been activated.

            if (active.ContainsKey(instance) == true)
            {
                return;
            }

            active.Add(instance, id);
            inactive[id].Remove(instance);
        }

        private void OnInstanceDeactivated(T instance, int id)
        {
            // short circuit if the instance has already been deactivated.

            if(inactive[id].Contains(instance) == true)
            {
                return;
            }
            active.Remove(instance);
            inactive[id].Add(instance);
        }


        /// <summary>
        /// Creates the container gameobject parent to hold all of the pooled gameobjects.
        /// </summary>

        protected void CreatePoolContainer()
        {
            poolContainer = new()
            {
                name = gameObject.name
            };

            poolContainer.transform.SetParent(GetPoolContainerParent());
        }

        /// <summary>
        /// Gets the GameObject parent that holds the poolContainer.
        /// </summary>
        /// <returns></returns>

        protected abstract Transform GetPoolContainerParent();

        private void OnDestroy()
        {
            // destroy all pooled objects along with this one.

            Destroy(poolContainer);
        }
    }
}
