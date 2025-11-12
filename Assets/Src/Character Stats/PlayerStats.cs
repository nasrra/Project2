using Entropek.EntityStats;
using Entropek.Physics;
using UnityEngine;

// needs to be before all components stored in here; so that base value setting occurs before component Awake or OnEnable.
// ensuring data is properly intialised.
[DefaultExecutionOrder(-1)]

public class PlayerStats : MonoBehaviour
{
    private const string MovementSpeedAnimationParameter = "MovementSpeed";
    private const string AttackSpeedAnimationParameter = "AttackSpeed";


    /// 
    /// Character Controller Movement Stats.
    /// 

    [Header("Character Controller Movement Stats.")]

    [SerializeField] CharacterControllerMovement characterControllerMovement;
    [SerializeField] Animator animator;

    [SerializeField] CharacterStatFloat runMaxSpeed = new();
    public CharacterStatFloat RunMaxSpeed => runMaxSpeed; 

    [SerializeField] CharacterStatFloat walkMaxSpeed = new();
    public CharacterStatFloat WalkMaxSpeed => walkMaxSpeed;

    [SerializeField] CharacterStatFloat acceleration = new();
    public CharacterStatFloat Acceleration => acceleration;
    
    [SerializeField] CharacterStatFloat deceleration = new();
    public CharacterStatFloat Deceleration => deceleration;

    [SerializeField] CharacterStatFloat skillMoveSpeedModifier = new();
    public CharacterStatFloat SkillMoveSpeedModifier => skillMoveSpeedModifier;


    ///
    /// Health Stats.
    /// 

    [Header("Health Stats.")]

    [SerializeField] Health health;

    [SerializeField] CharacterStatInt maxHealth = new();
    public CharacterStatInt MaxHealth => maxHealth;


    /// 
    /// Animator Stats.
    /// 

    [Header("Animator Stats.")]

    [SerializeField] CharacterStatFloat movementAnimationSpeed = new();
    public CharacterStatFloat MovementAnimationSpeed => movementAnimationSpeed;

    [SerializeField] CharacterStatFloat attackSpeed = new();
    public CharacterStatFloat AttackSpeed => attackSpeed;

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
        SetAnimatorBaseValues();
        SetHealthBaseValues();
    }

    private void SetCharacterControllerMovementBaseValues()
    {
        characterControllerMovement.SetMaxSpeed(walkMaxSpeed.BaseValue); // <-- player is always starts in the walk state.
        characterControllerMovement.SetAcceleration(acceleration.BaseValue);
        characterControllerMovement.SetDecleration(deceleration.BaseValue);
    }

    private void SetAnimatorBaseValues()
    {
        animator.SetFloat(MovementSpeedAnimationParameter, MovementAnimationSpeed.BaseValue);
        animator.SetFloat(AttackSpeedAnimationParameter, attackSpeed.BaseValue);
    }

    private void SetHealthBaseValues()
    {
        health.SetMaxValue(maxHealth.BaseValue);
    }

    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkCharacterControllerMovementEvents();
        LinkAnimatorEvents();
        LinkHealthEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkCharacterControllerMovementEvents();
        UnlinkAnimatorEvents();
        UnlinkHealthEvents();
    }


    /// 
    /// Character Controller Movement Linkage.
    /// 


    private void LinkCharacterControllerMovementEvents()
    {
        acceleration.ScaledValueCalculated += OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated += OnDecelerationScaledValueCalculated;
    }

    private void UnlinkCharacterControllerMovementEvents()
    {
        acceleration.ScaledValueCalculated -= OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated -= OnDecelerationScaledValueCalculated;        
    }

    private void OnAccelerationScaledValueCalculated(float value)
    {
        characterControllerMovement.SetAcceleration(value);
    }

    private void OnDecelerationScaledValueCalculated(float value)
    {
        characterControllerMovement.SetDecleration(value);
    }



    ///
    /// Animator Linkage.
    /// 


    private void LinkAnimatorEvents()
    {
        movementAnimationSpeed.ScaledValueCalculated += OnWalkAnimationSpeedScaledValueCalculated;
        attackSpeed.ScaledValueCalculated += OnAttackSpeedScaledValueCalculated;
    }

    private void UnlinkAnimatorEvents()
    {
        movementAnimationSpeed.ScaledValueCalculated -= OnWalkAnimationSpeedScaledValueCalculated;        
        attackSpeed.ScaledValueCalculated -= OnAttackSpeedScaledValueCalculated;
    }

    private void OnWalkAnimationSpeedScaledValueCalculated(float value)
    {
        animator.SetFloat(MovementSpeedAnimationParameter, value);
    }

    private void OnAttackSpeedScaledValueCalculated(float value)
    {
        animator.SetFloat(AttackSpeedAnimationParameter, value);        
    }


    ///
    /// Health Linkage.
    /// 

    
    private void LinkHealthEvents()
    {
        maxHealth.ScaledValueCalculated += OnMaxHealthScaledValueCalculated;
    }

    private void UnlinkHealthEvents()
    {
        maxHealth.ScaledValueCalculated -= OnMaxHealthScaledValueCalculated;        
    }

    private void OnMaxHealthScaledValueCalculated(int value)
    {
        health.SetMaxValue(value);
    }
}
