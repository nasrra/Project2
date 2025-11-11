using UnityEngine;

[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(AttackSpeedLinearModifierItem))]
public class AttackSpeedLinearModifierItem : Item
{
    [SerializeField] float modifierValue;
    [Tooltip(Item.EditorLinearModifierTooltip)]

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.AttackSpeed.AddLinearModifier(modifierValue);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.AttackSpeed.RemoveLinearModifier(modifierValue);
    }
}
