using Entropek.Time;
using UnityEngine;

public class ArcGhostMaterialController : MonoBehaviour{
    public Material material;
    public AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 3f),       // start (time=0)
        new Keyframe(0.2f, 0.75f, 3f, 0f),  // sharp rise.
        new Keyframe(0.3f, 1f, 0f, 0f),     // Plateau near max.
        new Keyframe(0.4f, 0.95f, 0f, -1f), // gentle decline.
        new Keyframe(0.6f, 0.9f, -2f, 0f)    // end at max time and value.
    );
    public Timer alphaTimer;

    void Awake(){
        material = GetComponent<MeshRenderer>().material;
        alphaTimer = gameObject.AddComponent<OneShotTimer>();
        alphaTimer.Begin(0.6f);
    }

    void FixedUpdate(){
        material.SetFloat("_Alpha", alphaCurve.Evaluate(alphaTimer.NormalisedCurrentTime));
    }
}
