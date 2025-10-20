using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Entropek.UnityUtils.AnimatorUtils{


[CustomEditor(typeof(AnimationEventStateBehaviour))]
public class AnimationEventStateBehaviourEditor : Editor{

    // animation to preview on the gameobject.

    AnimationClip previewClip;
    float previewTime;
    bool isPreviewing;

    public override void OnInspectorGUI(){
        
        DrawDefaultInspector();

        AnimationEventStateBehaviour stateBehaviour = (AnimationEventStateBehaviour) target;

        if(Validate(stateBehaviour, out string errorMessage)){
            
            // display the preview animation if we have found it.
            
            GUILayout.Space(10);

    

            if(isPreviewing){
                if(GUILayout.Button("Stop Preview")){
                    isPreviewing=false;
                    AnimationMode.StopAnimationMode();    
                }else{
                    PreviewAnimationClip(stateBehaviour);
                }
            }
            else if(GUILayout.Button("Start Preview")){
                isPreviewing = true;
                AnimationMode.StartAnimationMode();
            }


            GUILayout.Label($"Previewing at {previewTime:F2}s", EditorStyles.helpBox);
        
        }
        else{
            EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
        }
    }

    private void PreviewAnimationClip(AnimationEventStateBehaviour stateBehaviour){
        
        // sanity check.
        
        if(previewClip == null){
            return;
        }

        previewTime = stateBehaviour.TriggerTime * previewClip.length;

        // sample and set the state of the selected gameobject animator to that of the
        // animation clip at preview time. 

        AnimationMode.SampleAnimationClip(Selection.activeGameObject, previewClip, previewTime);
    }

    private bool Validate(AnimationEventStateBehaviour stateBehaviour, out string errorMessage){
        AnimatorController animatorController = GetValidAnimatorController(out errorMessage);
        if(animatorController==null){
            return false;
        }

        // find the state inside of the animator layers that contains
        // the target state behaviour to preview.
        // look for the first animation state in the animator state machine
        // where this state behaviour is present.

        ChildAnimatorState matchingState = animatorController.layers
            .SelectMany(layer => layer.stateMachine.states) 
            .FirstOrDefault(state => state.state.behaviours.Contains(stateBehaviour)); 


        // NOTE:
        //  presently do not support blend-trees.

        previewClip = matchingState.state?.motion as AnimationClip;
        if(previewClip == null){
            errorMessage = "No valid AnimationClip found for the current state.";
            return false;
        }
        return true;
    }

    private AnimatorController GetValidAnimatorController(out string errorMessage){
        errorMessage = string.Empty;

        // the gameobject that the user has selected in the editor hierarchy.

        GameObject selectedGameObject = Selection.activeGameObject;
        if(selectedGameObject==null){
            errorMessage = "Please select a GameObject with an Animator to preview.";
            return null;
        }

        Animator animator = selectedGameObject.GetComponent<Animator>();
        if(animator == null){
            errorMessage = "The selected GameObject does nt have an Animator component.";
            return null;
        }

        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if(animatorController == null){
            errorMessage = "The selected Animator does not have a vlaid AnimatorController.";
            return null;
        }

        return animatorController;
    }
}


}

