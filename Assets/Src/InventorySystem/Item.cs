using UnityEngine;

public abstract class Item : ScriptableObject
{
    #if UNITY_EDITOR

    /// <summary>
    /// The asset menu name for creating items in the unity editor.
    /// </summary>

    public const string EditorAssetMenuName = "ScriptableObject/InventorySystem/";
    public const string EditorLinearModifierTooltip = "+10% = 0.1";

    #endif

    [Header(nameof(Item)+" Data")]
    [SerializeField] ItemPickup itemPickupPrefab;
    public ItemPickup ItemPickupPrefab => itemPickupPrefab;

    public abstract void ApplyModifier(PlayerStats playerStats);
    public abstract void RemoveModifier(PlayerStats playerStats);

    /// <summary>
    /// Spawns the item prefab at an position and rotation in world space.
    /// </summary>
    /// <param name="position">The position to spawn at.</param>
    /// <param name="rotation">The rotation to spawn at.</param>

    public ItemPickup SpawnItemPickupPrefab(Vector3 position, Quaternion rotation)
    {
        return Instantiate(itemPickupPrefab, position, rotation);
    }
}

