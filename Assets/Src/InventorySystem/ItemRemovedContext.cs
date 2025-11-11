using UnityEngine;

public struct ItemRemovedContext
{
    public Item Item {get;}
    public float Amount {get;}

    public ItemRemovedContext(Item item, float amount)
    {
        Item = item;
        Amount = amount;
    }
}
