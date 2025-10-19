using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Entropek.Systems.Interaction{


[RequireComponent(typeof(SphereCollider))]
public class Interactor : MonoBehaviour{
    private List<Interactable> interactablesInSight = new List<Interactable>();
    private List<Interactable> interactablesNotInSight = new List<Interactable>();

    [Header("Data")]
    [SerializeField] LayerMask obstructionLayers;
    private int index;

    #if UNITY_EDITOR
    [Header("Editor")]
    [SerializeField] private bool drawSelected;
    #endif

    private void FixedUpdate(){
        VerifyInteractableInRange();

        for(int i = interactablesInSight.Count - 1; i >= 0; i--){
            
            Interactable interactable = interactablesInSight[i];
            

            if(InteractableInSight(interactable)==false){

                // tell the interactable that it has left our sights.
                // and swap to the out of sight list.

                interactablesNotInSight.Add(interactable);
                interactablesInSight.Remove(interactable);
                interactable.ExitInteractorSight();

                // clamp index to end of list if it is now out of range.

                if(index > 0 && index > interactablesInSight.Count - 1){
                    index = interactablesInSight.Count - 1;
                }
            }
        }

        for(int i = interactablesNotInSight.Count - 1; i >= 0; i--){
            
            Interactable interactable = interactablesNotInSight[i];
            
            if(InteractableInSight(interactable)==true){
    
                // tell the interactable that it has entered our sights.
                // and swap to in sight list.
    
                interactablesInSight.Add(interactable);
                interactablesNotInSight.Remove(interactable);
                interactable.EnterInteractorSight();
            }
        }

    }

    public void Interact(){

        // interact with the interactable at the current index.

        if(interactablesInSight.Count>0){
            interactablesInSight[index].Interact();
        }
    }

    private void OnTriggerEnter(Collider other){

        VerifyInteractableInRange();

        Interactable interactable = other.GetComponent<Interactable>();
        
        if(InteractableInSight(interactable)==true){
    
            // add the interactable if it is currently in sight.
    
            interactablesInSight.Add(interactable);
            interactable.EnterInteractorSight();
        }
    }

    private void OnTriggerExit(Collider other){
        
        VerifyInteractableInRange();
        
        Interactable interactable = other.GetComponent<Interactable>();

        // remove the interctable.

        interactablesInSight.Remove(interactable);
        
        // callback not-insight if it left our sight.

        if(interactable.IsInSight==true){
            interactable.ExitInteractorSight();
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
        index = (index + 1) % interactablesInSight.Count;
    }

    public void PreviousInteractable(){
        index = index - 1 < 0? interactablesInSight.Count-1 : index - 1;
    }

    private void VerifyInteractableInRange(){

        // remove any tracked interactables if they have been destroyed.

        interactablesInSight.RemoveAll(t=>t==null);
        interactablesNotInSight.RemoveAll(t=>t==null);
    }

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

