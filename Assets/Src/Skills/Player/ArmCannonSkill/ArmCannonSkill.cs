using System;
using System.Collections.Generic;
using Entropek;
using Entropek.Physics;
using Entropek.Time;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.UnityUtils.Attributes;
using TreeEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArmCannonSkill : Skill, ICooldownSkill, IAnimatedSkill
{

    /// 
    /// Constants.
    /// 

    private const string AnimationCompletedEventName = "ExitArmCannonState";
    private const string AnimationName = "ArmCannon";
    private const string ArmCannonBlastEventName = "ArmCannonBlast";
    private const float PullForceFactor = 4.8f;
    private const float PullFoceDecayFactor = PullForceFactor *(PullForceFactor * 1f);
    private const float PushForceViewFactor = 1.33f; // the factor to apply to push force when the camera is looking at an affected target.
    private const float PushForceFactor = 4.8f;
    private const float PushForceDecayFactor = PushForceFactor;
    private const float LowGravityModifier = 1;
    private const float JumpForce = 3.33f;
    private const float JumpForceDecay = JumpForce * JumpForce;


    /// 
    /// Components.
    /// 

    [Header("Components")]
    [DotProductRangeVisualise, SerializeField] private DotProductRange fov;


    /// 
    /// ICooldownSkill field overrides.
    /// 

    
    [SerializeField] OneShotTimer cooldownTimer;
    OneShotTimer ICooldownSkill.CooldownTimer => cooldownTimer;


    /// 
    /// IAnimatedSkill Field Overrides.
    /// 


    Skill IAnimatedSkill.Skill => this;

    Player IAnimatedSkill.Player => Player;

    public event Action AnimationCompleted;
    Action IAnimatedSkill.AnimationCompleted 
    { 
        get => AnimationCompleted; 
        set => AnimationCompleted = value; 
    }

    Animator IAnimatedSkill.Animator => Player.Animator;

    AnimationEventReciever IAnimatedSkill.AnimationEventReciever => Player.AnimationEventReciever;

    string IAnimatedSkill.AnimationName => AnimationName;

    string IAnimatedSkill.AnimationCompletedEventName => AnimationCompletedEventName;

    int IAnimatedSkill.AnimationLayer => 1;

    private Coroutine animationLayerWeightTransitionCoroutine;
    Coroutine IAnimatedSkill.AnimationLayerWeightTransitionCoroutine 
    { 
        get => animationLayerWeightTransitionCoroutine; 
        set => animationLayerWeightTransitionCoroutine = value; 
    }

    bool IAnimatedSkill.AnimationCancel => true;

    private float previousStateGravityModifier = 0;


    ///
    /// Unique Data. 
    /// 
    
    [Header("Unique Data")]
    [RuntimeField] Collider[] overlapColliders;
    [SerializeField] float attackRadius = 1;
    public float AttackRadius => attackRadius;
    [SerializeField] LayerMask obstructionLayers;
    [SerializeField] LayerMask effectedEntityLayers;


    /// 
    /// interface type cache.
    /// 


    ICooldownSkill ICooldownSkill;
    IAnimatedSkill IAnimatedSkill;


    public override bool CanUse()
    {
        return ICooldownSkill.CanUseCooldownSkill() && IAnimatedSkill.CanUseAnimatedSkill();
    }

    public void OnCooldownTimeout()
    {
        // do nothing.
    }

    protected override void GetInterfaceTypes()
    {
        ICooldownSkill = this;
        IAnimatedSkill = this;
    }

    protected override void UseInternal()
    {

        inUse = true;

        EnterLowGravityState();
        ApplyJumpForce();
        PullEntities();

        Player.CharacterControllerMovement.ClearGravityVelocity();

        Player.EnterWalkState();
        Player.BlockRunToggleInput();
        Player.BlockJumpInput();

        IAnimatedSkill.StartAnimationLayerWeightTransition(IAnimatedSkill.MaxAnimationLayerWeight, 100);
        IAnimatedSkill.PlayAnimation();
    }

    void PullEntities()
    {
        Vector3 origin = transform.position;

        // overlap sphere physics.

        overlapColliders = Physics.OverlapSphere(origin, attackRadius, effectedEntityLayers);
        
        // fire ray to each overlap, detecting if an obstruction is in the way.

        for(int i = 0; i < overlapColliders.Length; i++)
        {
            // get data relative to the detected collider.

            Collider other = overlapColliders[i];
            Vector3 otherPosition = other.transform.position;
            Vector3 vectorDistance = otherPosition - origin;
            Vector3 direction = vectorDistance.normalized;
            float distance = vectorDistance.magnitude;


            // check if any obstructions are in the way.

            if(Physics.Raycast(origin, direction, distance, obstructionLayers) == true)
            {
                continue;
            }

            if(other.TryGetComponent(out Minion minion))
            {                    
                PullCharacterControllerMovement(minion.NavAgentMovement, direction, distance);   
            }

            // do this last to ensure boss and minion components are always found;
            // as they both implement CharacterControllerMovement subclasses. 

            else if(other.TryGetComponent(out CharacterControllerMovement characterControllerMovement))
            {
                PullCharacterControllerMovement(characterControllerMovement, direction, distance);
            }
        }
    }

    void PushEntities()
    {
        Vector3 origin = transform.position;

        // overlap sphere physics.

        overlapColliders = Physics.OverlapSphere(origin, attackRadius, effectedEntityLayers);
        
        // fire ray to each overlap, detecting if an obstruction is in the way.

        for(int i = 0; i < overlapColliders.Length; i++)
        {
            // get data relative to the detected collider.

            Collider other = overlapColliders[i];
            Vector3 otherPosition = other.transform.position;
            Vector3 vectorDistance = otherPosition - origin;
            Vector3 direction = ApplyFlattening(vectorDistance.normalized, 0.66f);
            float distance = vectorDistance.magnitude;


            // check if any obstructions are in the way.

            if(Physics.Raycast(origin, direction, distance, obstructionLayers) == true)
            {
                continue;
            }

            if(other.TryGetComponent(out Minion minion))
            {                    
                PushCharacterControllerMovement(minion.NavAgentMovement, direction, distance);   
            }

            // do this last to ensure boss and minion components are always found;
            // as they both implement CharacterControllerMovement subclasses. 

            else if(other.TryGetComponent(out CharacterControllerMovement characterControllerMovement))
            {
                PushCharacterControllerMovement(characterControllerMovement, direction, distance);
            }
        }
    }

    void PullCharacterControllerMovement(CharacterControllerMovement character, Vector3 direction, float distance)
    {        
        // apply a force pulling the enemy to this position.
        // Note:
        //  The force is stronger the further away the enemy is.
        
        character.Impulse(-direction, distance * PullForceFactor, distance * PullFoceDecayFactor);
    }

    void PushCharacterControllerMovement(CharacterControllerMovement character, Vector3 direction, float distance)
    {
        Vector3 cameraForwardDirection = Player.CameraController.transform.forward;
        
        float dot = Vector3.Dot(direction, cameraForwardDirection);

        float force = PushForceFactor;
        float forceDecay = PushForceDecayFactor;

        // if the character is within view range of the camera.

        if(dot >= fov.Min)
        {
            // apply a greater force.
            
            force *= PushForceViewFactor;

            // push in the direction of the camera.

            direction = Vector3.MoveTowards(direction, cameraForwardDirection, dot);
        }



        // apply a force pushing the enemy away from this position.
        // Note:
        //  The force is stronger the closer the enemy is.
        float distanceFactor = (1 - distance / attackRadius) * attackRadius;

        character.Impulse(direction, distanceFactor * force, distanceFactor * forceDecay);
    }


    void EnterLowGravityState()
    {
        Player.CharacterControllerMovement.ClearGravityVelocity();
        previousStateGravityModifier = Player.CharacterControllerMovement.GravityModifier;
        Player.CharacterControllerMovement.SetGravityModifier(LowGravityModifier);
    }

    void ApplyJumpForce()
    {
        Player.CharacterControllerMovement.Impulse(Vector3.up, JumpForce, JumpForceDecay);
    }

    void ExitLowGravityState()
    {
        Player.CharacterControllerMovement.SetGravityModifier(previousStateGravityModifier);        
    }

    Vector3 ApplyFlattening(Vector3 direction, float amount)
    {
        Vector3 flat = new Vector3(direction.x, 0f, direction.z).normalized;
        return Vector3.MoveTowards(direction, flat, amount).normalized;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        ICooldownSkill.LinkCooldownSkillEvents();
        IAnimatedSkill.LinkAnimatedSkillEvents();
    }

    protected override void UnlinkEvents()
    {
        ICooldownSkill.UnlinkCooldownSkillEvents();
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
    }

    private void ArmCannonBlast()
    {
        ApplyJumpForce();        
        ExitLowGravityState();
        PushEntities();
    }

    void IAnimatedSkill.OnAnimationCompleted()
    {
        inUse = false;
        cooldownTimer.Begin();
        Player.UnblockRunToggleInput();
        Player.UnblockJumpInput();
        IAnimatedSkill.StartAnimationLayerWeightTransition(0, 100);
    }

    void IAnimatedSkill.Cancel()
    {
        throw new NotImplementedException();
    }

    void IAnimatedSkill.OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {
            case ArmCannonBlastEventName:
                ArmCannonBlast();
                break;
        }
    }
}
