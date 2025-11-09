using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-5)]
public class EnemyDirector : MonoBehaviour
{
    public static EnemyDirector Singleton {get; private set;}
   
    [Header("Components")]
    [SerializeField] RandomLoopedTimer evaluationTimer;

    [Header("Data")]
    [SerializeField] EnemySpawnCard[] spawnCards;
    private const float SlowEvaluationTimeMin = 10;
    private const float SlowEvaluationTimeMax = 20;
    private const float FastEvaluationTimeMin = 1;
    private const float FastEvaluationTimeMax = 2;

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
    
        FastState();
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

    int x = 5;

    public void Evaluate()
    {
        if (x > 5)
        {
            return;
        }

        x++;

        for(int j = 0; j < 20; j++)
        {            
            for(int i = 0; i < spawnCards.Length; i++)
            {
                EnemySpawnCard spawnCard = spawnCards[i];
                if(spawnCard.EnemyType == EnemyType.Minion)
                {
                    SpawnAtRandomPosition(spawnCard);
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

    public void SpawnMiniboss()
    {
        for(int i = 0; i < spawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = spawnCards[i];
            if(spawnCard.EnemyType == EnemyType.MiniBoss)
            {

                // spawn miniboss.

                GameObject miniboss = SpawnAtRandomPosition(spawnCard);
                
                // display boss health bar hud.
                
                Health health = miniboss.GetComponent<Health>();
                BossHealthBarHud.Singleton.NamedHealthBar.Activate(health, miniboss.name.Replace("(Clone)", ""));
                
                break;
            }
        }
    }

    public GameObject SpawnAtRandomPosition(EnemySpawnCard spawnCard)
    {
        Vector3 centerPosition = Opponent.Singleton.transform.position;

        Vector3 randomPosition = Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
            spawnCard.GetNavMeshQueryFilter(),
            centerPosition,
            SpawnRandomRadiusMin,
            SpawnRandomRadiusMax,
            SpawnQueryRadius,
            out bool foundPoint,
            iterations: 32
        );

        Vector3 spawnPosition = foundPoint==true? randomPosition : centerPosition; 
        return Instantiate(spawnCard.Prefab, position: spawnPosition, rotation: Quaternion.identity);
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
