using System;
using Entropek;
using Entropek.Camera;
using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Physics;
using Entropek.Time;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.UnityUtils.Attributes;
using Entropek.Vfx;
using UnityEngine;

public class ArmCannonSkill : Skill, ICooldownSkill, IAnimatedSkill
{


    /// 
    /// Constants.
    /// 


    private const string AnimationCompletedEventName = "ExitArmCannonState";
    private const string AnimationName = "ArmCannon";
    private const string ArmCannonBlastEventName = "ArmCannonBlast";
    private const string PushSfx = "ArmCannonSkillPush";
    private const string PullSfx = "ArmCannonSkillPull";

    private const float PullForceFactor = 4.8f;
    private const float PullForceDecayFactor = PullForceFactor *(PullForceFactor * 1f);
    private const float PushForceViewFactor = 1.33f; // the factor to apply to push force when the camera is looking at an affected target.
    private const float PushForceFactor = 4.8f;
    private const float PushForceDecayFactor = PushForceFactor;
    private const float LowGravityModifier = 1;
    private const float JumpForce = 3.33f;
    private const float JumpForceDecay = JumpForce * JumpForce;

    private const float PushCameraShakeForce = 8f;
    private const float PushCameraShakeTime = 0.24f;
    private const float PushLensDistortionIntensity = -0.48f;
    private const float PushLensDistortionDuration = 0.24f;
    private const float PushMotionBlurDuration = 0.24f;
    private const float PushMotionBlurIntensity = 1f;
    private const float PushCameraFov = CameraController.InitialFov + 10;
    private const float PushCameraFovLerpInDuration = 0.167f;
    private const float PushCameraFovLerpOutDuration = 0.33f;

    private const float PullCameraFov = CameraController.InitialFov - 10;
    private const float PullCameraFovLerpInDuration = 0.33f;

    private const int HitVfxId = 0;
    private const string HitSfx = "MeleeHit";

    private const int Damage = 0;
    private const DamageType TypeOfDamage = DamageType.Heavy;


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] VfxPlayerSpawner vfxPlayerSpawner;


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

    bool IAnimatedSkill.AllowsAnimationCancel => false;
    bool IAnimatedSkill.CausesAnimationCancel => true;

    private float previousStateGravityModifier = 0;


    ///
    /// Unique Data. 
    /// 
    
    [Header("Unique Data")]
    [RuntimeField] Collider[] overlapColliders;
    [DotProductRangeVisualise, SerializeField] private DotProductRange fov;
    [SerializeField] float attackRadius = 1;
    public float AttackRadius => attackRadius;
    [SerializeField] LayerMask obstructionLayers;
    [SerializeField] LayerMask effectedEntityLayers;


    /// 
    /// interface type cache.
    /// 


    ICooldownSkill ICooldownSkill;
    IAnimatedSkill IAnimatedSkill;


    /// 
    ///  Base.
    /// 


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
        IAnimatedSkill.UseAnimatedSkill();

        inUse = true;

        Player.FaceAttackDirection();

        EnterLowGravityState();
        ApplyJumpForce();
        PullEntities();

        Player.BlockRunToggleInput();
        Player.BlockJumpInput();
        Player.EnterWalkState();

        IAnimatedSkill.StartAnimationLayerWeightTransition(IAnimatedSkill.MaxAnimationLayerWeight, 100);

        Player.CameraController.StartLerpingFov(PullCameraFov, PullCameraFovLerpInDuration);

        Player.AudioPlayer.PlaySound(PullSfx, Player.transform);
    }


    ///
    /// Functions.
    /// 


    /// <summary>
    /// Queryies for unobstructed entities on the effectedEntityLayers; Pulling them towards this when successfully hit.
    /// </summary>

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
                PullCharacterControllerMovement(minion.NavAgentMovement, direction, distance, out _);   
            }

            // do this last to ensure boss and minion components are always found;
            // as they both implement CharacterControllerMovement subclasses. 

            else if(other.TryGetComponent(out CharacterControllerMovement characterControllerMovement))
            {
                PullCharacterControllerMovement(characterControllerMovement, direction, distance, out _);
            }
        }
    }

    /// <summary>
    /// Queryies for unobstructed entities on the effectedEntityLayers; Damaging and pushing away the successfully hit entities.
    /// </summary>

    void PushAndDamageEntities()
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

            if(other.TryGetComponent(out HealthSystem health))
            {
                health.Damage(new DamageContext(transform.position, Damage, TypeOfDamage));
                
                // calculate the point at which this attack will hit the entity.

                Vector3 hitPoint = transform.position + direction * distance;
                
                // play vfx and sfx.
                
                vfxPlayerSpawner.PlayVfx(HitVfxId,hitPoint);
                Player.AudioPlayer.PlaySound(HitSfx, health.transform.position);
            }

            if(other.TryGetComponent(out Minion minion))
            {
                if(PushCharacterControllerMovement(
                    minion.NavAgentMovement, 
                    direction, 
                    distance, 
                    out CharacterControllerMovementLinearImpulse impulse)
                )
                {
                    minion.EnterStunState(CalculatePushStunTime(impulse.Force, impulse.DecaySpeed));
                }
            }
            else if(other.TryGetComponent(out CharacterControllerMovement characterControllerMovement))
            {
                PushCharacterControllerMovement(characterControllerMovement, direction, distance, out _);
            }
        }
    }

    /// <summary>
    /// Applies an impulse force to a CharacterConctrollerMovement in the opposite direction of the supplied direction.
    /// Note:
    ///     The force applied grows stronger in relation to how far the distance is.
    ///     (distance = 0 is no force; increasing at farther distances.)
    /// </summary>
    /// <param name="character">The CharacterControllerMovement to apply the force to.</param>
    /// <param name="direction">The direction to move in the opposite direction from (away-direction)</param>
    /// <param name="distance">The distance the character is away from this gameobject.</param>
    /// <param name="impulse">The impulse force that was applied to the CharacterControllerMovement.</param>
    /// <returns>true, if a force was successfully applied; otherwise false.</returns>

    bool PullCharacterControllerMovement(CharacterControllerMovement character, Vector3 direction, float distance, out CharacterControllerMovementLinearImpulse impulse)
    {                
        float force = distance * PullForceFactor;
        float forceDecay = distance * PullForceDecayFactor;
        impulse = new();

        if(force > 0 && forceDecay > 0) // avoid edge cases where the distance is at the attack radius.
        {
            impulse = new(-direction, force, forceDecay);
            character.Impulse(impulse);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pushes a Character in the supplied direction.
    /// Note:
    ///     The force applied grows stronger in relation to how close the distance is. 
    ///     (distance = 0 is full force; diminishing at farther distances).
    /// </summary>
    /// <param name="character">The CharacterControllerMovement to apply the force to.</param>
    /// <param name="direction">The direction to move in.</param>
    /// <param name="distance">The distance the character is away from this gameobject.</param>
    /// <param name="impulse">The impulse force that was applied to the CharacterControllerMovement.</param>
    /// <returns>true, if a force was successfully applied; otherwise false.</returns>

    bool PushCharacterControllerMovement(CharacterControllerMovement character, Vector3 direction, float distance, out CharacterControllerMovementLinearImpulse impulse)
    {

        float force = PushForceFactor;
        float forceDecay = PushForceDecayFactor;

        // if the character is within view range of the camera.

        Vector3 cameraForwardDirection = Player.CameraController.transform.forward;
        float dot = Vector3.Dot(direction, cameraForwardDirection);

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
        force *= distanceFactor;
        forceDecay *= distanceFactor;

        
        if(force > 0 && forceDecay > 0) // avoid edge cases where the distance is at the attack radius.
        {
            impulse = new CharacterControllerMovementLinearImpulse(direction, force, forceDecay);
            character.Impulse(impulse);
            return true;
        }

        impulse = new();
        return false;
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

    /// <summary>
    /// Flattens a normalized Vector3 to its x and z components based on an amount.
    /// Note:
    ///     amount = 0 is no flattening, amount = 1 is full flattening.
    /// </summary>
    /// <param name="direction">The normalised direction to flatten.</param>
    /// <param name="amount">The amount to flatten it by.</param>
    /// <returns>The flattened noramlised direction.</returns>

    Vector3 ApplyFlattening(Vector3 direction, float amount)
    {
        Vector3 flat = new Vector3(direction.x, 0f, direction.z).normalized;
        return Vector3.MoveTowards(direction, flat, amount).normalized;
    }

    /// <summary>
    /// Calculates the time to stun an Minion depending on an impulse force and its decay speed.
    /// Note:
    ///     the stun time increases when the decay factor is lower then the force factor.
    /// </summary>
    /// <param name="force"></param>
    /// <param name="forceDecay"></param>
    /// <returns>The time (in seconds) to stun the Minion.</returns>

    private float CalculatePushStunTime(float force, float forceDecay)
    {    
        // force divide by speed equals time.

        return PushForceFactor / PushForceDecayFactor + (1 - forceDecay / force);
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
        PushAndDamageEntities();
        PlayBlastVfxAndSfx();
    }

    private void PlayBlastVfxAndSfx()
    {
        Player.AudioPlayer.PlaySound(PushSfx, Player.transform);
        Player.CameraController.StartShaking(PushCameraShakeForce, PushCameraShakeTime);

        Player.CameraPostProcessingController.PulseLensDistortionIntensity(
            PushLensDistortionDuration,
            PushLensDistortionIntensity
        );

        Player.CameraPostProcessingController.PulseMotionBlurIntensity(
            PushMotionBlurDuration,
            PushMotionBlurIntensity
        );

        Player.CameraController.StartLerpingFov(
            PushCameraFov, 
            PushCameraFovLerpInDuration,
            ()=>{Player.CameraController.StartLerpingFov(CameraController.InitialFov, PushCameraFovLerpOutDuration);});        
    }

    void IAnimatedSkill.OnAnimationCompleted()
    {
        inUse = false;
        cooldownTimer.Begin();
        IAnimatedSkill.StartAnimationLayerWeightTransition(0, 100);
        Player.UnblockRunToggleInput();
        Player.UnblockJumpInput();
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
