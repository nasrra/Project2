using Entropek.Combat;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDirector : MonoBehaviour
{
    public static EnemyDirector Singleton {get; private set;}
   
    [Header("Components")]
    [SerializeField] LoopedTimer evaluationTimer;

    [Header("Data")]
    [SerializeField] EnemySpawnCard[] spawnCards;
    private const float SlowEvaluationTimeMin = 20;
    private const float SlowEvaluationTimeMax = 30;
    private const float FastEvaluationTimeMin = 5;
    private const float FastEvaluationTimeMax = 10;


    /// 
    /// Base.
    /// 


    void Awake()
    {
        if (Singleton != null)
        {
            throw new SingletonException(nameof(EnemyDirector));
        }
        else
        {
            Singleton = this;
        }

        // Evaluate();

        LinkEvents();
    }

    void FixedUpdate()
    {
        
    }

    void OnDestroy()
    {
        if(Singleton == this)
        {
            Singleton = null;
        }

        UnlinkEvents();
    }


    ///
    /// Functions.
    /// 


    public void Evaluate()
    {
        for(int j = 0; j < 2; j++)
        {            
            for(int i = 0; i < spawnCards.Length; i++)
            {
                EnemySpawnCard spawnCard = spawnCards[i];

                Vector3 randomPosition = Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
                    new NavMeshQueryFilter()
                    {
                        areaMask = NavMesh.AllAreas,
                        agentTypeID = spawnCard.NavMeshAgentType
                    },
                    Opponent.Singleton.transform.position,
                    40,
                    1f,
                    out bool foundPoint
                );

                if(foundPoint == true)
                {
                    Instantiate(spawnCard.Prefab, position: randomPosition, rotation: Quaternion.identity);
                }
            }
        }
    }


    /// 
    /// Linkage.
    /// 


    protected void LinkEvents()
    {
        LinkTimerEvents();
    }

    protected void UnlinkEvents()
    {
        UnlinkTimerEvents();
    }

    protected void LinkTimerEvents()
    {
        evaluationTimer.Timeout += Evaluate; 
    }

    protected void UnlinkTimerEvents()
    {
        evaluationTimer.Timeout -= Evaluate;         
    }
}
