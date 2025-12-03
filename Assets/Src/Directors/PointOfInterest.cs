using System;
using UnityEngine;

public class PointOfInterst : MonoBehaviour
{
    public event Action<int> Destroyed;

    public int LocationId;

    void OnDestroy()
    {
        Destroyed?.Invoke(LocationId);
        Destroyed = null;
    }
}
