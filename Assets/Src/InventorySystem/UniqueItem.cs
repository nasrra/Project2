using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(UniqueItem))]
#endif
public class UniqueItem : Item
{
    [Header(nameof(UniqueItem)+" Data")]
    [SerializeField] private UniqueCharacterStat uniqueCharacterStatPrefab;

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.AddUniqueCharacterStat(uniqueCharacterStatPrefab);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.RemoveUniqueCharacterStat(uniqueCharacterStatPrefab);
    }
}
