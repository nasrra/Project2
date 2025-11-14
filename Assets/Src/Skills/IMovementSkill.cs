using UnityEngine;

public interface IMovementSkill
{
    PlayerStats PlayerStats{get;}

    void LinkMovementSkillEvents()
    {
        PlayerStats.SkillMoveSpeedModifier.ScaledValueCalculated += OnCalculatedScaledMoveSpeedModifier;
    }

    void UnlinkMovementSkillEvents()
    {
        PlayerStats.SkillMoveSpeedModifier.ScaledValueCalculated -= OnCalculatedScaledMoveSpeedModifier;        
    }

    void OnCalculatedScaledMoveSpeedModifier(float value);
}
