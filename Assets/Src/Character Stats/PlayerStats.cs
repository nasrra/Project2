using Entropek.EntityStats;
using Entropek.Physics;
using UnityEngine;

// needs to be before all components stored in here; so that base value setting occurs before component Awake or OnEnable.
// ensuring data is properly intialised.
[DefaultExecutionOrder(-1)]

public class PlayerStats : MonoBehaviour
{
    private const string WalkSpeedAnimationParameter = "WalkSpeed";
    private const string AttackSpeedAnimationParameter = "AttackSpeed";


    /// 
    /// Character Controller Movement Stats.
    /// 

    [Header("Character Controller Movement Stats.")]

    [SerializeField] CharacterControllerMovement characterControllerMovement;
    [SerializeField] Animator animator;

    [SerializeField] CharacterStatFloat maxSpeed = new();
    public CharacterStatFloat MaxSpeed => maxSpeed;

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

    [SerializeField] CharacterStatFloat walkAnimationSpeed = new();
    public CharacterStatFloat WalkAnimationSpeed => walkAnimationSpeed;

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
        characterControllerMovement.SetMaxSpeed(maxSpeed.BaseValue);
        characterControllerMovement.SetAcceleration(acceleration.BaseValue);
        characterControllerMovement.SetDecleration(deceleration.BaseValue);
    }

    private void SetAnimatorBaseValues()
    {
        animator.SetFloat(WalkSpeedAnimationParameter, walkAnimationSpeed.BaseValue);
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
        maxSpeed.ScaledValueCalculated += OnMaxSpeedScaledValueCalculated;
        acceleration.ScaledValueCalculated += OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated += OnDecelerationScaledValueCalculated;
    }

    private void UnlinkCharacterControllerMovementEvents()
    {
        maxSpeed.ScaledValueCalculated -= OnMaxSpeedScaledValueCalculated;
        acceleration.ScaledValueCalculated -= OnAccelerationScaledValueCalculated;
        deceleration.ScaledValueCalculated -= OnDecelerationScaledValueCalculated;        
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



    ///
    /// Animator Linkage.
    /// 


    private void LinkAnimatorEvents()
    {
        walkAnimationSpeed.ScaledValueCalculated += OnWalkAnimationSpeedScaledValueCalculated;
        attackSpeed.ScaledValueCalculated += OnAttackSpeedScaledValueCalculated;
    }

    private void UnlinkAnimatorEvents()
    {
        walkAnimationSpeed.ScaledValueCalculated -= OnWalkAnimationSpeedScaledValueCalculated;        
        attackSpeed.ScaledValueCalculated -= OnAttackSpeedScaledValueCalculated;
    }

    private void OnWalkAnimationSpeedScaledValueCalculated(float value)
    {
        animator.SetFloat(WalkSpeedAnimationParameter, value);
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
