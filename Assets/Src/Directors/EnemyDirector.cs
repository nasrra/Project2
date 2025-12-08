using Entropek.Combat;
using Entropek.Exceptions;
using Entropek.Time;
using UnityEngine;
using UnityEngine.AI;
using Entropek.UnityUtils.Attributes;
using System.Collections.Generic;
using UnityEngine.UIElements;

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

    private const int MiniBossDeathGoldAwardMin = 25;
    private const int MiniBossDeathGoldAwardMax = 32; 


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] RandomLoopedTimer evaluationTimer;
    [SerializeField] private CreditDirector creditDirector;
    public CreditDirector CreditDirector => creditDirector;


    /// 
    /// Data.
    /// 


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

    public void Evaluate()
    {
        Debug.Log("Eval");

        float maxCost = enemySpawnCardCollection.MaxCost;
        float minCost = enemySpawnCardCollection.MinCost;
        maxCost = UnityEngine.Random.Range(minCost,maxCost*1.5f); // <-- add a 50% buffer so that the higher cost enemies are chosen more often.
        enemySpawnCardCollection.SetSpawnCardsSpawnableByCostRange(
            Mathf.FloorToInt(minCost), 
            Mathf.CeilToInt(maxCost)
        );

        ExecuteSpawnBehaviour(Opponent.Singleton.transform.position, creditDirector.Credits, UnityEngine.Random.Range(0,4), out creditDirector.Credits);
    
    }

    /// <summary>
    /// Performs a spawn enemy operation at a position in world-space.
    /// Note:
    ///     Each bevahiour spawns a random amount of enemies depending on the credits this directed has accumulated.
    ///     0 -> Spawn Many of Random Enemy (may fall back to any other behvaiour when failing).
    ///     1 -> Spawn High to Low Cost.
    ///     2 -> Spawn Low to High Cost.
    ///     4 etc -> Spawn All Uniform.
    /// </summary>
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>
    /// <param name="credits">The amount of credits to spend when spawning.</param>
    /// <param name="spawnBehvaiourId">The id of the behvaiour to perform.</param>
    /// <param name="creditsRemainder">The amount credits at the end of this spawn operation.</param>
    
    public void ExecuteSpawnBehaviour(Vector3 centerPosition, float credits, int spawnBehvaiourId, out float creditsRemainder)
    {        
        switch (spawnBehvaiourId)
        {
            case 0:
                // Debug.Log("Spawn Many");
                if(SpawnManyOfRandomEnemy(
                    centerPosition,
                    credits, 
                    enemySpawnCardCollection.EnemySpawnCards.Length, 
                    out creditsRemainder
                ) == false)
                {

                    // execute another random spawn behvaiour if this one fails.

                    ExecuteSpawnBehaviour(centerPosition, credits, UnityEngine.Random.Range(1,4), out creditsRemainder);
                } 
                break;
            case 1:
                // Debug.Log("Spawn High To Low");
                SpawnHighestToLowestCostEnemiesBatched(centerPosition, credits, out creditsRemainder);
                break;
            case 2:
                // Debug.Log("Spawn Low To High");
                SpawnLowestToHighestCostEnemiesBatched(centerPosition, credits, out creditsRemainder);
                break;
            default:
                // Debug.Log("Spawn Uniform");
                SpawnAllEnemiesUniform(centerPosition, credits, out creditsRemainder);
                break;
        }
    }
    
    /// <summary>
    /// Spawns as many of a random spawnable enemy as possible.
    /// </summary>
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>
    /// <param name="credits">The amount of credits to spend when spawning.</param>
    /// <param name="iterations">The amount of iterations to perform for finding a random spawnable spawn card before failing.</param>
    /// <param name="creditsRemainder">The amount credits at the end of this spawn operation.</param>
    /// <returns>true, if a spawnable spawn card was found and spawned; otherwise false.</returns>
    
    public bool SpawnManyOfRandomEnemy(Vector3 centerPosition, float credits, int iterations, out float creditsRemainder)
    {
        for(int i = 0; i <= iterations; i++)
        {
            int index = UnityEngine.Random.Range(0, enemySpawnCardCollection.EnemySpawnCards.Length);
            EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[index];

            if (spawnCard.Spawnable == true && spawnCard.Cost <= credits)
            {
                while(credits >= spawnCard.Cost)
                {
                    if(SpawnAtRandomPosition(spawnCard, centerPosition, out GameObject instantiatedGameObject))
                    {
                        credits -= spawnCard.Cost;
                    }
                }
                creditsRemainder = credits;
                return true;
            }
        }

        creditsRemainder = credits;
        return false;

    }

    /// <summary>
    /// Spawns spawnable enemy spawn cards from highest to lowest; spawning as many high cards as possible, then casacding down the list.
    /// </summary>    
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>
    /// <param name="credits">The amount of credits to spend when spawning spawn cards.</param>
    /// <param name="creditsRemaineder">The amount credits at the end of this spawn operation.</param>
    
    public void SpawnHighestToLowestCostEnemiesBatched(Vector3 centerPosition, float credits, out float creditsRemaineder)
    {
        for(int i = enemySpawnCardCollection.EnemySpawnCards.Length - 1; i > 0; i--)
        {
            
            EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i]; 
            
            if(spawnCard.Spawnable == false)
            {
                continue;
            }

            while(credits >= spawnCard.Cost)
            {
                SpawnAtRandomPosition(spawnCard, centerPosition, out GameObject minion);
                credits -= spawnCard.Cost;
            }
        }

        creditsRemaineder = credits;
    }

    /// <summary>
    /// Spawns spawnable enemy spawn cards from lowest to highest; spawning as many high cards as possible, then casacding down the list.
    /// </summary>
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>    
    /// <param name="credits">The amount of credits to spend when spawning spawn cards.</param>
    /// <param name="creditsRemainder">The amount credits at the end of this spawn operation.</param>
    
    public void SpawnLowestToHighestCostEnemiesBatched(Vector3 centerPosition, float credits, out float creditsRemainder)
    {
        for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
        {
            
            EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i]; 
            
            if(spawnCard.Spawnable == false)
            {
                continue;
            }

            while(credits >= spawnCard.Cost)
            {
                SpawnAtRandomPosition(spawnCard, centerPosition, out GameObject minion);
                credits -= spawnCard.Cost;
            }
        }

        creditsRemainder = credits;
    }

    /// <summary>
    /// Spawns all spawnable spawn cards in the stored enemy spawn card collection that are spawnable.
    /// Spawning one of each spawn card from highest to lowest.
    /// Note: 
    ///  The spawns can be multiple when credits are high enough or none when credits are low enough.
    ///  It is an approximation of a uniform spread across spawnable spawn cards.
    /// </summary>
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>
    /// <param name="credits">The amount of credits to spend when spawning spawn cards.</param>
    /// <param name="creditsRemained">The amount credits at the end of this spawn operation.</param>

    public void SpawnAllEnemiesUniform(Vector3 centerPosition, float credits, out float creditsRemained)
    {
        bool spawnedEnemy = true;
        while(credits > 0 && spawnedEnemy)
        {
            spawnedEnemy = false;
            for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
            {
                EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i];
                if( spawnCard.Spawnable == true
                &&  spawnCard.Cost <= credits)
                {
                    // spawn enemy.

                    if(SpawnAtRandomPosition(spawnCard, centerPosition, out GameObject minion) == false)
                    {
                        continue;
                    }
                    credits -= spawnCard.Cost;
                    spawnedEnemy = true;
                    Debug.Log($"Spawned {minion.name}");
                }
            }
        }

        creditsRemained = credits;
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
    /// <param name="centerPosition">The point (in world-space) to start the random search at.</param>
    /// <param name="instantiatedGameObject">The instantiated enemy gameobject.</param>
    /// <returns>true, if a random point was found to spawn the enemy at.</returns>
    
    public bool SpawnAtRandomPosition(EnemySpawnCard spawnCard, Vector3 centerPosition, out GameObject instantiatedGameObject)
    {
        instantiatedGameObject = null;

        // Vector3 centerPosition = Opponent.Singleton.transform.position;

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
        // Debug.Log(instantiatedGameObject.name);
        return true;
    }

    // public void SpawnMiniboss()
    // {
    //     for(int i = 0; i < enemySpawnCardCollection.EnemySpawnCards.Length; i++)
    //     {
    //         EnemySpawnCard spawnCard = enemySpawnCardCollection.EnemySpawnCards[i];
    //         if(spawnCard.EnemyType == EnemyType.MiniBoss)
    //         {

    //             // spawn miniboss.
                
    //             if(SpawnAtRandomPosition(spawnCard, out GameObject miniboss)==false)
    //             {
    //                 continue;
    //             }

    //             // display boss health bar hud.
                
    //             Health health = miniboss.GetComponent<Health>();
    //             BossHealthBarHud.Singleton.NamedHealthBar.Activate(health, miniboss.name.Replace("(Clone)", ""));
                
    //             break;
    //         }
    //     }
    // }


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
