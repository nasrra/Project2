using UnityEngine;

[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(MaxHealthLinearModifierItem))]
public class MaxHealthLinearModifierItem : Item
{
    [Tooltip(Item.EditorLinearModifierTooltip)]
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
