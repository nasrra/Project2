using UnityEngine;

[CreateAssetMenu(menuName = Item.EditorAssetMenuName+nameof(LinearModifierMovementItem))]
public class LinearModifierMovementItem : Item
{
    [Tooltip("+10% = 0.1")]
    [SerializeField] float modifierValue;

    public override void ApplyModifier(PlayerStats playerStats)
    {
        playerStats.MaxSpeed.AddLinearModifier(modifierValue);
        playerStats.Acceleration.AddLinearModifier(modifierValue);
        playerStats.Deceleration.AddLinearModifier(modifierValue);
        playerStats.WalkAnimationSpeed.AddLinearModifier(modifierValue);
        playerStats.SkillMoveSpeedModifier.AddLinearModifier(modifierValue);
    }

    public override void RemoveModifier(PlayerStats playerStats)
    {
        playerStats.MaxSpeed.RemoveLinearModifier(modifierValue);
        playerStats.Acceleration.RemoveLinearModifier(modifierValue);
        playerStats.Deceleration.RemoveLinearModifier(modifierValue);
        playerStats.WalkAnimationSpeed.RemoveLinearModifier(modifierValue);
        playerStats.SkillMoveSpeedModifier.RemoveLinearModifier(modifierValue);
    }
}
