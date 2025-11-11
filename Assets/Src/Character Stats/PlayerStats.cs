using Entropek.Physics;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private const string WalkSpeedAnimationParameter = "WalkSpeed";

    [SerializeField] CharacterControllerMovement characterControllerMovement;
    [SerializeField] Animator animator;

    [SerializeField] CharacterStatFloat maxSpeed = new();
    public CharacterStatFloat MaxSpeed => maxSpeed;

    [SerializeField] CharacterStatFloat acceleration = new();
    public CharacterStatFloat Acceleration => acceleration;
    
    [SerializeField] CharacterStatFloat deceleration = new();
    public CharacterStatFloat Deceleration => deceleration;

    [SerializeField] CharacterStatFloat walkAnimationSpeed = new();
    public CharacterStatFloat WalkAnimationSpeed => walkAnimationSpeed;

    [SerializeField] CharacterStatFloat skillMoveSpeedModifier = new();
    public CharacterStatFloat SkillMoveSpeedModifier => skillMoveSpeedModifier;


    private void Awake()
    {
        LinkEvents();
        SetBaseValues();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }

    private void SetBaseValues()
    {
        SetCharacterControllerMovementBaseValues();
    }

    private void SetCharacterControllerMovementBaseValues()
    {
        characterControllerMovement.SetMaxSpeed(maxSpeed.BaseValue);
        characterControllerMovement.SetAcceleration(acceleration.BaseValue);
        characterControllerMovement.SetDecleration(deceleration.BaseValue);
        animator.SetFloat(WalkSpeedAnimationParameter, walkAnimationSpeed.BaseValue);
    }

    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkCharacterControllerMovementEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkCharacterControllerMovementEvents();
    }


    /// 
    /// Character Controller Movement Linkage.
    /// 


    private void LinkCharacterControllerMovementEvents()
    {
        maxSpeed.ScaledValueCalculated += OnMaxSpeedScaledValueCalculated;
        acceleration.ScaledValueCalculated += OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated += OnDecelerationScaledValueCalculated;
        walkAnimationSpeed.ScaledValueCalculated += OnWalkAnimationSpeedScaledValueCalculated;
    }

    private void UnlinkCharacterControllerMovementEvents()
    {
        maxSpeed.ScaledValueCalculated -= OnMaxSpeedScaledValueCalculated;
        acceleration.ScaledValueCalculated -= OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated -= OnDecelerationScaledValueCalculated;        
        walkAnimationSpeed.ScaledValueCalculated -= OnWalkAnimationSpeedScaledValueCalculated;
    }

    private void OnMaxSpeedScaledValueCalculated(float value)
    {
        characterControllerMovement.SetMaxSpeed(value);
    }

    private void OnAccelerationScaledValueCalculated(float value)
    {
        characterControllerMovement.SetAcceleration(value);
    }

    private void OnDecelerationScaledValueCalculated(float value)
    {
        characterControllerMovement.SetDecleration(value);
    }

    private void OnWalkAnimationSpeedScaledValueCalculated(float value)
    {
        animator.SetFloat(WalkSpeedAnimationParameter, value);
    }
}
