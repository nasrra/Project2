using System;
using System.Collections.Generic;
using UnityEngine;

// NOTE:
//  This should be attached to a gameobject with a collider,
//  where the gameobject has a local position of 0,0,0 in relation to the parent.

[RequireComponent(typeof(Collider))]
public class LockOnTargetDetector : MonoBehaviour{
    
    public event Action TargetLeftRange;
    private List<Transform> detectedTransforms = new List<Transform>();
    private Transform currentTarget;
    private Transform followTarget; 
    private int index;
    private const float SimilarityThreshold = 0.8f;

    private void FixedUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

    void OnTriggerEnter(Collider other){
    
        VerifyDetectedTransforms();

        detectedTransforms.Add(other.transform);
    }

    void OnTriggerExit(Collider other)
    {

        VerifyDetectedTransforms();

        if (other.transform == currentTarget)
        {
            TargetLeftRange?.Invoke();
        }

        detectedTransforms.Remove(other.transform);
    }

    public void SetFollowTarget(Transform followTarget)
    {
        this.followTarget = followTarget;
    }

    public Transform GetTarget(Vector3 forwardDirection){
        
        VerifyDetectedTransforms();
        
        int closestTransformIndex = -1; // the closest transform to the mouse direction.
        float strongestSimilarity = float.MinValue;        

        for(int i = 0; i < detectedTransforms.Count; i++){
            Vector3 direction = (detectedTransforms[i].position - transform.position).normalized;
            float similarity = Vector3.Dot(direction, forwardDirection);
            
            // if the transform is within the view angle
            // and is closer than the previous transform.
            
            if(SimilarityThreshold < similarity 
            && strongestSimilarity < similarity){
                strongestSimilarity = similarity;
                closestTransformIndex = i;
            }
        }

        index = closestTransformIndex;

        currentTarget = closestTransformIndex==-1? null: detectedTransforms[index];
        
        return currentTarget;
    }

    public Transform GetNext(){

        VerifyDetectedTransforms();

        index = (index+1)%(detectedTransforms.Count-1);
        
        currentTarget = detectedTransforms[index];
        
        return currentTarget;
    }

    public Transform GetPrevious(){
        
        VerifyDetectedTransforms();

        index--;
        
        if(index < 0){
            index = detectedTransforms.Count-1;
        }
        
        currentTarget = detectedTransforms[index];
        
        return currentTarget;
    }

    private void VerifyDetectedTransforms(){
        
        // loop through each of the detected transforms
        // to check if any are null after being call Destroy() on;
        // removing it if so.

        detectedTransforms.RemoveAll(t => t == null);
    }

}
