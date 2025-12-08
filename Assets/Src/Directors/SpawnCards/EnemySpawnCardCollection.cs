using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = CreateAssetMenuPath+nameof(EnemySpawnCardCollection))]
#endif

public class EnemySpawnCardCollection : ScriptableObject
{

#if UNITY_EDITOR
    private const string CreateAssetMenuPath = "ScriptableObject/Directors/";
#endif

    [Tooltip("The cost of enemy spawn cards should be arrange from lowest to highest: index 0 = lowest -> index MAX = highest.")]
    [SerializeField] private EnemySpawnCard[] enemySpawnCards;
    
    [SerializeField] private float maxCost;
    public float MaxCost => maxCost; 
    
    [SerializeField] private float minCost;
    public float MinCost => minCost;
    
    public EnemySpawnCard[] EnemySpawnCards => enemySpawnCards;  


    /// <summary>
    /// Sets each spawn cards spawnable to true if it is within the cost range; other wise false.
    /// </summary>
    /// <param name="minCost">The minimum cost for a spawn card to be considered spawnable.</param>
    /// <param name="maxCost">The maximum cost for a spawn card to be considered spawnable.</param>

    public void SetSpawnCardsSpawnableByCostRange(float minCost, float maxCost)
    {
        // Debug.Log($"{minCost} min, {maxCost} max");
        for(int i = 0; i < enemySpawnCards.Length; i++)
        {
            SpawnCard spawnCard = enemySpawnCards[i];
            spawnCard.Spawnable = spawnCard.Cost >= minCost && spawnCard.Cost <= maxCost;
        }
    }

    /// <summary>
    /// Sorts the enemy spawn cards array to be in descending order by their cost values.
    /// </summary>

    public void SortByCostDescending()
    {
        Array.Sort(enemySpawnCards, (a,b) => a.Cost.CompareTo(b.Cost));
    }


    /// <summary>
    /// Gets the minimum and maximum cost of all cards in this collection.
    /// </summary>
    /// <param name="minCost">The minimum cost.</param>
    /// <param name="maxCost">The maximum cost.</param>
    /// <returns>true, if the collection has found a min and max value; otherwise false.</returns>

    public bool SetMinAndMaxCardCost()
    {

        minCost = float.MaxValue;
        maxCost = float.MinValue;

        for (int i = 0; i < enemySpawnCards.Length; i++)
        {
            EnemySpawnCard spawnCard = enemySpawnCards[i];
            
            float cost = spawnCard.Cost;
            
            if(minCost > cost)
            {
                minCost = cost;
            }

            if(maxCost < cost)
            {
                maxCost = cost;
            }

        }

        return true;
    }
}
