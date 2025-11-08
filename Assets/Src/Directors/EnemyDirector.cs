using Entropek.Combat;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDirector : MonoBehaviour
{
    public static EnemyDirector Singleton {get; private set;}
   
    [Header("Components")]
    [SerializeField] RandomLoopedTimer evaluationTimer;

    [Header("Data")]
    [SerializeField] EnemySpawnCard[] spawnCards;
    private const float SlowEvaluationTimeMin = 10;
    private const float SlowEvaluationTimeMax = 20;
    private const float FastEvaluationTimeMin = 5;
    private const float FastEvaluationTimeMax = 10;

    private const float SpawnRandomRadiusMin = 16.4f;
    private const float SpawnRandomRadiusMax = 48;
    private const float SpawnQueryRadius = 3.33f;



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
    
        SlowState();
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
                    spawnCard.GetNavMeshQueryFilter(),
                    Opponent.Singleton.transform.position,
                    SpawnRandomRadiusMin,
                    SpawnRandomRadiusMax,
                    SpawnQueryRadius,
                    out bool foundPoint
                );

                if(foundPoint == true)
                {
                    Instantiate(spawnCard.Prefab, position: randomPosition, rotation: Quaternion.identity);
                }
            }
        }
    }

    public void FastState()
    {
        evaluationTimer.SetInitialTimeRange(FastEvaluationTimeMin, FastEvaluationTimeMax);
        evaluationTimer.Begin();
    }

    public void SlowState()
    {
        evaluationTimer.SetInitialTimeRange(SlowEvaluationTimeMin, SlowEvaluationTimeMax);        
        evaluationTimer.Begin();
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
