using System;
using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class CameraAimTarget : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private new Transform camera;
    [SerializeField] private LayerMask hitLayers;
    [TagSelector] private string ignoreTag;
    RaycastHit[] hits = new RaycastHit[10]; // max 10 hits.

    void FixedUpdate()
    {
        Array.Clear(hits, 0, hits.Length);
        if(Physics.RaycastNonAlloc(camera.transform.position, camera.transform.forward, hits, float.MaxValue, hitLayers) > 0)
        {
            // sort the list as RaycastNonAlloc does not guarantee ordering by distance.
            Array.Sort(hits, 0, hits.Length, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
            
            for(int i = 0; i < hits.Length; i++)
            {
                // get a reference to this possible hit detection.

                ref RaycastHit hit = ref hits[i];

                Transform hitTransform = hit.transform;

                if(hitTransform == null
                || hitTransform.tag == null
                || hitTransform.tag == ignoreTag)
                {
                    continue;
                }

                transform.position = hit.point;
                break;
            }
        }
        else
        {
            transform.position = camera.transform.position + camera.transform.forward * 100;
        }
    }
}
