using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/InventorySystem/ItemDropPool")]
public class ItemDropPool : ScriptableObject
{
    [SerializeField] Item[] commonItems;
    [SerializeField] Item[] rareItems;
    [SerializeField] Item[] legendaryItems;

    public Item GetRandomCommonItem()
    {
        return commonItems[Random.Range(0, commonItems.Length)];
    }    

    public Item GetRandomRateItem()
    {
        return rareItems[Random.Range(0, rareItems.Length)];
    }

    public Item GetRandomLegendaryItem()
    {
        return legendaryItems[Random.Range(0, legendaryItems.Length)];
    }
}
