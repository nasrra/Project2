using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] Transform target;

    // Update is called once per frame
    void Update()
    {
        agent.Warp(target.position);
        agent.Move(Vector3.zero);
    }
}
