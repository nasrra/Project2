using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = CreateAssetMenuPath+nameof(EnemySpawnCardCollection))]
#endif

public class EnemySpawnCardCollection : ScriptableObject
{

#if UNITY_EDITOR
    private const string CreateAssetMenuPath = "ScriptableObject/Directors/";
#endif

    [SerializeField] private EnemySpawnCard[] enemySpawnCards;
    public EnemySpawnCard[] EnemySpawnCards => enemySpawnCards;  

}
