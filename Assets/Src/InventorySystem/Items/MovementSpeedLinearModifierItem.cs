using UnityEngine;

[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(MovementSpeedLinearModifierItem))]
public class MovementSpeedLinearModifierItem : Item
{
    [Tooltip("+10% = 0.1")]
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
