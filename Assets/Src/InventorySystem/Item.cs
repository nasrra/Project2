using UnityEngine;

public abstract class Item : ScriptableObject
{
    #if UNITY_EDITOR

    /// <summary>
    /// The asset menu name for creating items in the unity editor.
    /// </summary>

    public const string EditorAssetMenuName = "ScriptableObject/InventorySystem/";

    #endif

    // NOTE:
    //  ID is used for hash coding in Dictionaries and equality checks.

    [SerializeField] private ushort id;
    public ushort Id => id;

    public abstract void ApplyModifier(PlayerStats playerStats);
    public abstract void RemoveModifier(PlayerStats playerStats);

    public override bool Equals(object obj)
    {
        // check for null and type.
        if(obj == null|| obj.GetType() != this.GetType())
        {
            return false;
        }

        // compare ID's
        Item other = (Item)obj;
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        // use Id as the hash code.
        return Id.GetHashCode();
    }

    public static bool operator ==(Item a, Item b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        else if (a == null || b == null)
        {
            return false;
        }
        return a.Id == b.Id;

    }

    public static bool operator !=(Item a, Item b)
    {
        return !(a==b);
    }
}

