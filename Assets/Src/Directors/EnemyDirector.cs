using System.Net;
using System;
using Entropek.Collections;
using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;
using Entropek.UnityUtils.Attributes;
using System.Collections.Generic;

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


    private const float SlowEvaluationTimeMin = 7.5f;
    private const float SlowEvaluationTimeMax = 15;
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
    [SerializeField] CreditDirector creditDirector;

    [Header("Data")]
    
    [Tooltip("The currently used enemy spawn card collection used for enemy spawning.")]
    [RuntimeField] EnemySpawnCardCollection enemySpawnCardCollection;
    
    private List<int> spawnableEnemies = new();

    [Tooltip("The enemy spawn card collection that is used depending upon an elapsed minute variable; E.g id 1 = minute 1, id 3 = minute 3")]
    [SerializeField] EnemySpawnCardCollection[] enemySpawnCardCollectionPerElapsedMinute;
    
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
        SlowState();
        SetEnemySpawnCardCollection(0);
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
        float maxCost = enemySpawnCardCollection.MaxCost;
        float minCost = enemySpawnCardCollection.MinCost;
        maxCost = UnityEngine.Random.Range(minCost,maxCost*1.25f); // <-- add a 25% buffer so that the higher cost enemies are chosen more often.
        minCost = UnityEngine.Random.Range(minCost,maxCost);

        enemySpawnCardCollection.SetSpawnCardsSpawnableByCostRange(minCost, maxCost);

        bool spawnedEnemy = true;
        while(creditDirector.Credits > 0 && spawnedEnemy)
        {
            spawnedEnemy = false;
            for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
            {
                EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i];
                if(spawnCard.Cost <= creditDirector.Credits)
                {
                    // spawn enemy.

                    if(SpawnAtRandomPosition(spawnCard, out GameObject minion) == false)
                    {
                        continue;
                    }

                    Enemy enemy = minion.GetComponent<Enemy>();
                    enemy.Health.Death += OnMinionDeath;
                    
                    creditDirector.Credits -= spawnCard.Cost;

                    spawnedEnemy = true;

                    Debug.Log($"Spawned {enemy.name}");
                }
            }
        }

    }


    ///
    /// Spawn Card Handling.
    /// 


    /// <summary>
    /// Sets the currently used enemy spawn card collection to a collection stored in the internal array.
    /// </summary>
    /// <param name="collectionId">The id of the collection in the internal array.</param>

    public void SetEnemySpawnCardCollection(int collectionId)
    {
        enemySpawnCardCollection = enemySpawnCardCollectionPerElapsedMinute[collectionId];
        
    }

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

        bool foundPoint = Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
            spawnCard.GetNavMeshQueryFilter(),
            centerPosition,
            SpawnRandomRadiusMin,
            SpawnRandomRadiusMax,
            SpawnQueryRadius,
            out NavMeshHit point,
            iterations: 32
        );

        if (foundPoint == false)
        {
            return false;
        }

        Vector3 spawnPosition = point.position;

        // spawn aerial enemies at their base height value.
        
        spawnPosition += spawnCard.SpawnType == EnemySpawnType.Aerial
        ? Vector3.up * NavMesh.GetSettingsByID(spawnCard.NavMeshAgentType).agentHeight
        : Vector3.zero;
        
        instantiatedGameObject = Instantiate(spawnCard.Prefab, position: spawnPosition, rotation: Quaternion.identity);
        return true;
    }

    public void SpawnMiniboss()
    {
        for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i];
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
        for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i];
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
        LinkPlaythroughStopwatchEvents();
    }

    protected void UnlinkEvents()
    {
        UnlinkTimerEvents();
        UnlinkPlaythroughStopwatchEvents();
    }

    protected void ClearEvents()
    {
        AwardCurrency = null;
    }

    
    /// 
    /// Timer Event Linkage.
    /// 


    protected void LinkTimerEvents()
    {
        evaluationTimer.Timeout += Evaluate; 
    }

    protected void UnlinkTimerEvents()
    {
        evaluationTimer.Timeout -= Evaluate;         
    }


    /// 
    /// PlaythroughStopwatch Event Linkage.
    /// 


    protected void LinkPlaythroughStopwatchEvents()
    {
        PlaythroughStopwatch.Singleton.ElapsedMinute += OnElapsedMinute;
    }

    protected void UnlinkPlaythroughStopwatchEvents()
    {
        if (PlaythroughStopwatch.Singleton != null)
        {
            PlaythroughStopwatch.Singleton.ElapsedMinute -= OnElapsedMinute;        
        }
    }

    private void OnElapsedMinute(int elapsedMinutes)
    {
        if(elapsedMinutes < enemySpawnCardCollectionPerElapsedMinute.Length)
        {
            SetEnemySpawnCardCollection(elapsedMinutes);
        }
    }
}
