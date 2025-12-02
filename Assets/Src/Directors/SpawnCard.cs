using Entropek.UnityUtils.Attributes;
using UnityEngine;
using UnityEngine.AI;

public abstract class SpawnCard : ScriptableObject
{
    protected const string CreateAssetMenuPath = "ScriptableObject/Directors/";

    [Header(nameof(SpawnCard))]
    
    [Tooltip("The prefab to instantiate.")]
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [Tooltip("The cost to instantiate the prefab.")]
    [SerializeField] private float cost;
    public float Cost => cost;

    [Tooltip("The nav mesh used to find a position to place the prefab")]
    [SerializeField, NavMeshAgentTypeField] private int navMeshAgentType;
    public int NavMeshAgentType => navMeshAgentType;

    public NavMeshQueryFilter GetNavMeshQueryFilter()
    {
        return new NavMeshQueryFilter()
        {
            areaMask =  NavMesh.AllAreas,
            agentTypeID = navMeshAgentType  
        };
    }
}
