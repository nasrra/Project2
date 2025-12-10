using UnityEngine;

public abstract class UniqueCharacterStat : MonoBehaviour
{
    [SerializeField] private byte id;
    public byte Id => id;
    protected int amount;
    public int Amount => amount;

    /// <summary>
    /// Intialises this UniqueCharacterStat in relation to a PlayerStats instance.
    /// </summary>
    /// <param name="playerStats"></param>

    public abstract void Initialise(PlayerStats playerStats);

    /// <summary>
    /// Increments the base stat values by the items scaling factors.
    /// </summary>
    /// <param name="amount">The amount to increment by.</param>

    public abstract void AddModifier(int amount);

    /// <summary>
    /// Decrements the base stat values by the items scaling factors.
    /// </summary>
    /// <param name="amount"></param>

    public abstract void RemoveModifier(int amount);
}
