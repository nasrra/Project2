using UnityEngine;

public interface IMovementSkill
{
    PlayerStats PlayerStats{get;}
    float MoveSpeedModifier {get; protected set;}

    void LinkMovementSkillEvents()
    {
        PlayerStats.SkillMoveSpeedModifier.ScaledValueCalculated += OnCalculatedScaledMoveSpeedModifier;
    }

    void UnlinkMovementSkillEvents()
    {
        PlayerStats.SkillMoveSpeedModifier.ScaledValueCalculated -= OnCalculatedScaledMoveSpeedModifier;        
    }

    void OnCalculatedScaledMoveSpeedModifier(float value)
    {
        MoveSpeedModifier = value;
    }

    float ApplyMoveSpeedModifier(float value)
    {
        return value * MoveSpeedModifier;
    }
}
