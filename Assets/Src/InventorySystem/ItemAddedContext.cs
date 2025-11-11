using UnityEngine;

public struct ItemAddedContext
{
    public Item Item {get;}
    public float Amount {get;}

    public ItemAddedContext(Item item, float amount)
    {
        Item = item;
        Amount = amount;
    }
}
