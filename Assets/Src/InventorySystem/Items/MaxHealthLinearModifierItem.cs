using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(MaxHealthLinearModifierItem))]
#endif
public class MaxHealthLinearModifierItem : Item
{
    #if UNITY_EDITOR
    [Tooltip(Item.EditorLinearModifierTooltip)]
    #endif

    [SerializeField] float modifierValue;

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.MaxHealth.AddLinearModifier(modifierValue);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.MaxHealth.RemoveLinearModifier(modifierValue);
    }
}
