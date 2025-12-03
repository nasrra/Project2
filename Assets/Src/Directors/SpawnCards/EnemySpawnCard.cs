using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath+nameof(EnemySpawnCard))]
public class EnemySpawnCard : SpawnCard
{
    [Header("Data")]
    [SerializeField] private EnemyType enemyType;
    public EnemyType EnemyType => enemyType;

    [SerializeField] private EnemySpawnType spawnType;
    public EnemySpawnType SpawnType => spawnType;
}
