using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    private const float MaxDropChance = 100;

    [SerializeField] ItemDropPool itemDropPoolScriptableObject;

    [SerializeField][Range(0,MaxDropChance)] private float commonItemDropChance = 0;
    [SerializeField][Range(0,MaxDropChance)] private float rareItemDropChance = 0;
    [SerializeField][Range(0,MaxDropChance)] private float legendaryItemDropChance = 0;

    public void DropItem()
    {
        float rng = Random.Range(0, MaxDropChance+1); // add 1 because max is exclusive.        
    
        Item droppedItem;

        if(rng <= commonItemDropChance)
        {
            droppedItem = itemDropPoolScriptableObject.GetRandomCommonItem();
        }
        else if(rng <= rareItemDropChance)
        {
            droppedItem = itemDropPoolScriptableObject.GetRandomRateItem();
        }
        else
        {
            droppedItem = itemDropPoolScriptableObject.GetRandomLegendaryItem();
        }

        Debug.Log(droppedItem.name);
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        EditorClampTotalDropChanceValue();        
    }

    private void EditorClampTotalDropChanceValue()
    {
        float sum = commonItemDropChance + rareItemDropChance + legendaryItemDropChance;
        if(sum > MaxDropChance)
        {
            float scale = MaxDropChance / sum;

            commonItemDropChance *= scale;
            commonItemDropChance = commonItemDropChance > 0.1f 
            ? commonItemDropChance 
            : 0;

            rareItemDropChance *= scale;
            rareItemDropChance = rareItemDropChance > 0.1f 
            ? rareItemDropChance
            : 0;

            legendaryItemDropChance *= scale;
            legendaryItemDropChance = legendaryItemDropChance > 0.1f
            ? legendaryItemDropChance
            : 0;
        }
    }

#endif

}
