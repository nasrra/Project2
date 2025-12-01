using System;
using Entropek.Collections;
using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-5)]
public class EnemyDirector : MonoBehaviour
{


    /// 
    /// Singleton.
    /// 


    public static EnemyDirector Singleton {get; private set;}


    /// 
    /// Constants.
    /// 


    private const float SlowEvaluationTimeMin = 10;
    private const float SlowEvaluationTimeMax = 20;
    private const float FastEvaluationTimeMin = 5;
    private const float FastEvaluationTimeMax = 10;

    private const float SpawnRandomRadiusMin = 16.4f;
    private const float SpawnRandomRadiusMax = 48;
    private const float SpawnQueryRadius = 3.33f;

    private const int MinionDeathGoldAwardMin = 10;
    private const int MinionDeathGoldAwardMax = 17;
    private const int MiniBossDeathGoldAwardMin = 25;
    private const int MiniBossDeathGoldAwardMax = 32; 


    /// 
    /// Callbacks.
    /// 


    public event Action<Currency, int> AwardCurrency;


    [Header("Components")]
    [SerializeField] RandomLoopedTimer evaluationTimer;

    [Header("Data")]
    [SerializeField] EnemySpawnCard[] spawnCards;
    [SerializeField] Currency awardedCurrency;


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

        LinkEvents();
    
        // FastState();
    }

    void OnDestroy()
    {
        if(Singleton == this)
        {
            Singleton = null;
        }

        UnlinkEvents();
        ClearEvents();
    }


    ///
    /// State Machine.
    /// 
    
    
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

    // int x = 0;
    public void Evaluate()
    {

        for(int j = 0; j < 1; j++)
        {            
            SpawnMinion();
        }
    }


    ///
    /// Spawn Card Handling.
    /// 


    /// <summary>
    /// Spawns the prefab stored within a EnemySpawn card at a random location determined by its nav agent AgentTypeId.
    /// </summary>
    /// <param name="spawnCard">The specified EnemySpawnCard to evaluatate.</param>
    /// <param name="instantiatedGameObject">The instantiated enemy gameobject.</param>
    /// <returns>true, if a random point was found to spawn the enemy at.</returns>

    public bool SpawnAtRandomPosition(EnemySpawnCard spawnCard, out GameObject instantiatedGameObject)
    {
        instantiatedGameObject = null;

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

        if (foundPoint == false)
        {
            return false;
        }

        Vector3 spawnPosition = randomPosition;

        // spawn aerial enemies at their base height value.
        
        spawnPosition += spawnCard.SpawnType == EnemySpawnType.Aerial
        ? Vector3.up * NavMesh.GetSettingsByID(spawnCard.NavMeshAgentType).agentHeight
        : Vector3.zero;
        
        instantiatedGameObject = Instantiate(spawnCard.Prefab, position: spawnPosition, rotation: Quaternion.identity);
        return true;
    }

    public void SpawnMiniboss()
    {
        for(int i = 0; i < spawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = spawnCards[i];
            if(spawnCard.EnemyType == EnemyType.MiniBoss)
            {

                // spawn miniboss.
                
                if(SpawnAtRandomPosition(spawnCard, out GameObject miniboss)==false)
                {
                    continue;
                }

                // display boss health bar hud.
                
                Health health = miniboss.GetComponent<Health>();
                BossHealthBarHud.Singleton.NamedHealthBar.Activate(health, miniboss.name.Replace("(Clone)", ""));
                
                break;
            }
        }
    }

    public void SpawnMinion()
    {
        for(int i = 0; i < spawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = spawnCards[i];
            if(spawnCard.EnemyType == EnemyType.Minion)
            {
                // spawn enemy.

                if(SpawnAtRandomPosition(spawnCard, out GameObject minion) == false)
                {
                    continue;
                }

                Enemy enemy = minion.GetComponent<Enemy>();
                enemy.Health.Death += OnMinionDeath;
            }
        }
    }


    ///
    /// Enemy Death Handling.
    /// 


    private void OnMinionDeath()
    {
        int amount = UnityEngine.Random.Range(MinionDeathGoldAwardMin, MinionDeathGoldAwardMax + 1); // add one as its exclusive.
        AwardCurrency?.Invoke(awardedCurrency, amount);
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

    protected void ClearEvents()
    {
        AwardCurrency = null;
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
