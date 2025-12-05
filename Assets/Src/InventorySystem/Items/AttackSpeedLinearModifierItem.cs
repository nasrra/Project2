using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(AttackSpeedLinearModifierItem))]
#endif
public class AttackSpeedLinearModifierItem : Item
{

    #if UNITY_EDITOR
    [Tooltip(Item.EditorLinearModifierTooltip)]
    #endif
    [SerializeField] float modifierValue;

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.AttackSpeed.AddLinearModifier(modifierValue);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.AttackSpeed.RemoveLinearModifier(modifierValue);
    }
}
