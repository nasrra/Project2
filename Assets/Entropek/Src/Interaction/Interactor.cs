using System.Collections.Generic;
using Entropek.Exceptions;
using Entropek.Physics;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Interaction{


    public class Interactor : MonoBehaviour{

        [RuntimeField] private List<Interactable> interactablesInSight          = new List<Interactable>();
        [RuntimeField] private List<Interactable> interactablesNotInSight       = new List<Interactable>();
        [RuntimeField] private List<Interactable> interactablesInRange          = new List<Interactable>();
        
        [Header("Components")]
        [SerializeField] private ColliderListener inSightTriggerCollider;
        [SerializeField] private ColliderListener inRangeTriggerCollider;

        [Header("Data")]
        [Tooltip("The gameobject at the top of the local hierarchy that holds this interactor (Player, Enemy, item, etc.)")]
        [SerializeField] private GameObject rootGameObject;
        public GameObject RootGameObject => rootGameObject;

        [SerializeField] LayerMask obstructionLayers;
        private int index;

#if UNITY_EDITOR

        [Header("Editor")]
        [SerializeField] private bool drawSelected;

#endif

        private void Awake()
        {
            LinkEvents();
        }

        private void FixedUpdate()
        {
            VerifyInteractables();
            InteractablesInSightUpdateTick();
            InteractablesNotInSightUpdateTick();
            InteractablesInRangeUpdateTick();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }

        /// 
        /// Functions.
        /// 


        private void InteractablesInSightUpdateTick()
        {            
            for(int i = interactablesInSight.Count - 1; i >= 0; i--)
            {
                Interactable interactable = interactablesInSight[i];

                if (InteractableInSight(interactable) == false)
                {
                    interactablesInSight.Remove(interactable);
                    interactablesNotInSight.Add(interactable);
                    interactable.ExitInteractorSight(this);
                }
            }
        }

        private void InteractablesNotInSightUpdateTick()
        {
            for(int i = interactablesNotInSight.Count - 1; i >= 0; i--)
            {
                Interactable interactable = interactablesNotInSight[i];

                if (InteractableInSight(interactable) == false)
                {
                    interactablesNotInSight.Remove(interactable);
                    interactablesInSight.Add(interactable);
                    interactable.EnterInteractorSight(this);
                }
            }
        }

        private void InteractablesInRangeUpdateTick()
        {
            for(int i = interactablesInRange.Count - 1; i >= 0; i--)
            {
                Interactable interactable = interactablesInRange[i];

                if (InteractableInSight(interactable) == false)
                {
                    interactablesInRange.Remove(interactable);
                    interactablesNotInSight.Add(interactable);
                    interactable.ExitInteractorRange(this);
                }
            }
        }

        private void ClampInteractionIndex()
        {
            // clamp index to end of list if it is now out of range.

            if(index > 0 && index > interactablesInRange.Count - 1){
                index = interactablesInRange.Count - 1;
            }            
        }

        public void Interact(){

            // check for nulls and clamp index so the data is always 'up-to-date';
            // even when an Update calls this before this classes FixedUpdate call. 

            VerifyInteractables();
            ClampInteractionIndex();

            // interact with the interactable at the current index.

            if(interactablesInRange.Count>0){
                interactablesInRange[index].Interact(this);
            }
        }

        private bool InteractableInSight(Interactable interactable){
            Vector3 distance = interactable.transform.position - transform.position;
            Vector3 direction = distance.normalized;
            return !UnityEngine.Physics.Raycast(
                transform.position, 
                direction, 
                out RaycastHit hit, 
                distance.magnitude, 
                obstructionLayers, 
                QueryTriggerInteraction.Collide
            );
            
        }

        public void NextInteractable(){
            index = (index + 1) % interactablesInRange.Count;
        }

        public void PreviousInteractable(){
            index = index - 1 < 0? interactablesInRange.Count-1 : index - 1;
        }

        /// <summary>
        /// Removes any tracked Interactables when they have been detroyed by UnityEngine OR they are no longer interactable. 
        /// </summary>

        private void VerifyInteractables()
        {
            interactablesInSight.RemoveAll(t=>t==null || t.IsInteractable==false);
            interactablesNotInSight.RemoveAll(t=>t==null || t.IsInteractable==false);
            interactablesInRange.RemoveAll(t=>t==null || t.IsInteractable==false);
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents()
        {
            LinkColliderListenerEvents();   
        }

        private void UnlinkEvents()
        {
            UnlinkColliderListenerEvents();
        }

        private void LinkColliderListenerEvents()
        {
            inSightTriggerCollider.TriggerEnter += OnInSightColliderListenerTriggerEnter;
            inSightTriggerCollider.TriggerExit += OnInSightColliderListenerTriggerExit;
            inRangeTriggerCollider.TriggerEnter += OnInRangeColliderListenerTriggerEnter;
            inRangeTriggerCollider.TriggerExit += OnInRangeColliderListenerTriggerExit;
        }

        private void UnlinkColliderListenerEvents()
        {
            inSightTriggerCollider.TriggerEnter -= OnInSightColliderListenerTriggerEnter;
            inSightTriggerCollider.TriggerExit -= OnInSightColliderListenerTriggerExit;            
            inRangeTriggerCollider.TriggerEnter -= OnInRangeColliderListenerTriggerEnter;
            inRangeTriggerCollider.TriggerExit -= OnInRangeColliderListenerTriggerExit;
        }

        private void OnInSightColliderListenerTriggerEnter(Collider other)
        {
            Interactable interactable = other.GetComponent<Interactable>();

#if UNITY_EDITOR

            if(interactable == null)
            {
                throw new ComponentNotFoundException($"{other.name} does not contain the Interactable component.");                
            }

#endif

            if(InteractableInSight(interactable)==true){
        
                // add the interactable if it is currently in sight.
        
                interactablesInSight.Add(interactable);
                interactable.EnterInteractorSight(this);
            }

            VerifyInteractables();
        }

        private void OnInSightColliderListenerTriggerExit(Collider other)
        {            
            Interactable interactable = other.GetComponent<Interactable>();

            // remove the interactable.

            interactablesInSight.Remove(interactable);
            
            // callback not-insight if it left our sight.

            if(interactablesInSight.Contains(interactable)){
                interactable.ExitInteractorSight(this);
            }

            VerifyInteractables();
        }

        private void OnInRangeColliderListenerTriggerEnter(Collider other)
        {
            Interactable interactable = other.GetComponent<Interactable>();

            interactablesInRange.Add(interactable);
            
            if (interactablesInSight.Contains(interactable))
            {
                interactable.EnterInteractorRange(this);
            }

            ClampInteractionIndex();

            VerifyInteractables();
        }

        private void OnInRangeColliderListenerTriggerExit(Collider other)
        {
            Interactable interactable = other.GetComponent<Interactable>();

            interactablesInRange.Remove(interactable);
            
            interactable.ExitInteractorRange(this);

            ClampInteractionIndex();         
        
            VerifyInteractables();
        }


        /// 
        /// Debug Editor.
        /// 


#if UNITY_EDITOR

        private void OnDrawGizmos(){
            if(drawSelected==false){
                return;
            }
        
            // draw a sphere at the selected (indexed) interactable position.
            
            if(interactablesInSight.Count>0 && index < interactablesInSight.Count){
                Interactable interactable = interactablesInSight[index];
                if(interactable!=null){
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(interactablesInSight[index].transform.position, 0.33f);
                }
            }
        }

#endif

    }


}

