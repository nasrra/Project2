using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(MovementSpeedLinearModifierItem))]
#endif

public class MovementSpeedLinearModifierItem : Item
{
    #if UNITY_EDITOR
    [Tooltip(Item.EditorLinearModifierTooltip)]
    #endif
    [SerializeField] float modifierValue;

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.RunMaxSpeed.AddLinearModifier(modifierValue);
        playerStats.WalkMaxSpeed.AddLinearModifier(modifierValue);
        playerStats.Acceleration.AddLinearModifier(modifierValue);
        playerStats.Deceleration.AddLinearModifier(modifierValue);
        playerStats.MovementAnimationSpeed.AddLinearModifier(modifierValue);
        playerStats.SkillMoveSpeedModifier.AddLinearModifier(modifierValue);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.RunMaxSpeed.RemoveLinearModifier(modifierValue);
        playerStats.WalkMaxSpeed.RemoveLinearModifier(modifierValue);
        playerStats.Acceleration.RemoveLinearModifier(modifierValue);
        playerStats.Deceleration.RemoveLinearModifier(modifierValue);
        playerStats.MovementAnimationSpeed.RemoveLinearModifier(modifierValue);
        playerStats.SkillMoveSpeedModifier.RemoveLinearModifier(modifierValue);
    }
}
