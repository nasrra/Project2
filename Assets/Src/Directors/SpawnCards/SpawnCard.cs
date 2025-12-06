using Entropek.UnityUtils.Attributes;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = CreateAssetMenuPath+nameof(SpawnCard))]
#endif

public class SpawnCard : ScriptableObject
{

#if UNITY_EDITOR
    protected const string CreateAssetMenuPath = "ScriptableObject/Directors/";
#endif

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

    [SerializeField] public bool Spawnable = true; 

    public NavMeshQueryFilter GetNavMeshQueryFilter()
    {
        return new NavMeshQueryFilter()
        {
            areaMask =  NavMesh.AllAreas,
            agentTypeID = navMeshAgentType  
        };
    }
}
