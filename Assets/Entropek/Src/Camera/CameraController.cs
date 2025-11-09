using System;
using UnityEngine;

namespace Entropek.Camera
{

    public class CameraController : MonoBehaviour
    {

        private event Action Shake;
        public event Action Rotated;

        Quaternion previousRotation;
        Quaternion lookRotation;

        [Header("Componentes")]
        [SerializeField] private new UnityEngine.Camera camera;
        public UnityEngine.Camera Camera => camera; 
        [SerializeField] private Transform shaker;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform lockOnTarget;
        [SerializeField] private Entropek.Time.LoopedTimer shakeTimer;
        [SerializeField] private LockOnTargetDetector lockOnTargetDetector; // this must be a separate game object childed to this one.


        [Header("Data")]
        private Vector3 smoothedFollowPosition;
        private Vector3 desiredFollowDirection;
        private Vector3 targetShakerOffset;
        private Vector3 lookMovementDelta;
        [SerializeField] private Vector3 followOffset;
        private Vector2 inputSensitivity;
        public Vector2 MouseSensitivity;
        public Vector2 JoystickSensitivity;
        [SerializeField] private LayerMask obstructionLayer;
        private float shakeStrength;

        private const float FollowSpeed = 13.33f;
        private const float MouseInputSmoothSpeed = 100f;
        private const float LockOnTargetSmoothSpeed = 16.7f;
        private const float UpperPitchLimit = 70f;
        private const float LowerPitchLimit = -45f;
        private const float CameraRadius = 0.33f;
        private const float LookAtLockOnTargetSmoothSpeed = 6.66f;
        private const float LookMovementDeltaTransitionSpeed = 1000;

        public bool isLockedOn { get; private set; }


        /// 
        /// Base.
        /// 


        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (followTarget != null)
            {
                SnapToPosition(followTarget.position);
                SetFollowTarget(followTarget);
            }

            InitialiseInputSensitivity();

            LinkEvents();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }

        private void LateUpdate()
        {

            // move towards the target look movement delta over delta time; to ensure camera sensitivity remains the same
            // across variable frame rates.

            // lookMovementDelta = Vector3.MoveTowards(lookMovementDelta, InputManager.Singleton.LookDelta, UnityEngine.Time.deltaTime * LookMovementDeltaTransitionSpeed);
            ApplyDeltaMovementToLookRotation(InputManager.Singleton.LookDelta);
            
            
            // smoothly lerp to the follow targets position.

            CalculateSmoothedFollowPosition();
            Vector3 relativeOffset = CalculateOffsetRelativeToLookRotation();
            Vector3 offsetedSmoothedFollowPosition = ApplyOffsetToFollowPosition(smoothedFollowPosition, relativeOffset);
            transform.position = CalculateDeltaPosition(offsetedSmoothedFollowPosition);

            // apply mouse rotation.

            transform.rotation = CalculateRotationToRelativeTarget();

            // update shaker position.

            shaker.localPosition = CalculateShakerDeltaLocalPosition(targetShakerOffset);

            CheckIfRotated();
        }


        private void FixedUpdate()
        {
            Shake?.Invoke();
        }


        /// 
        /// Setters.
        /// 


        /// <summary>
        /// Sets the follow offset for the camera. 
        /// </summary>
        /// <param name="followOffset">The specified offset.</param>

        public void SetFollowOffset(Vector3 followOffset)
        {
            this.followOffset = followOffset;
        }

        /// <summary>
        /// Sets the target to follow.
        /// </summary>
        /// <param name="followTarget">The specified target to follow</param>

        public void SetFollowTarget(Transform followTarget)
        {
            this.followTarget = followTarget;
            lockOnTargetDetector.SetFollowTarget(followTarget);
        }

        /// <summary>
        /// Sets the target to lock onto (look at) whilst moving.
        /// </summary>
        /// <param name="lockOnTarget">The specified targett to look at.</param>

        public void SetLockOnTarget(Transform lockOnTarget)
        {
            isLockedOn = !(lockOnTarget == null);
            this.lockOnTarget = lockOnTarget;
        }


        /// 
        /// Positional Updating.
        /// 


        /// <summary>
        /// Snaps the camera to a position in world space; with follow offsets applied.
        /// </summary>
        /// <param name="position">The position in world space to snap to.</param>

        public void SnapToPosition(Vector3 position)
        {
            // update the smoothed position as well,
            // so it can smooth back to its resting point.

            smoothedFollowPosition = position;

            Vector3 relativeOffset = CalculateOffsetRelativeToLookRotation();
            transform.position = ApplyOffsetToFollowPosition(position, relativeOffset);
            transform.rotation = CalculateRotationToRelativeTarget();
        }

        /// <summary>
        /// Calculates a lerped Vector3 position between the current smoothedFollowPosition and follow target's position in world space. 
        /// </summary>

        private void CalculateSmoothedFollowPosition()
        {
            smoothedFollowPosition = Vector3.Lerp(smoothedFollowPosition, followTarget.position, FollowSpeed * UnityEngine.Time.deltaTime);
        }

        /// <summary>
        /// Returns a Vector3 of the follow offset projected onto the look rotations axis.
        /// </summary>
        /// <returns>The follow offset relative to the look rotation.</returns>

        private Vector3 CalculateOffsetRelativeToLookRotation()
        {
            if (lockOnTarget != null)
            {

                // offset the camera in relation to the lock on target.
                // otherwise offset the camera in relation to the mouse input.

                Vector3 directionToLockOnTarget = (lockOnTarget.position - transform.position).normalized;

                lookRotation = Quaternion.LookRotation(directionToLockOnTarget, Vector3.up);

            }

            return lookRotation * followOffset;
        }

        /// <summary>
        /// Applies an offset to the follow position, ensuring the camera does not clip through any obstructions along the way.
        /// </summary>
        /// <param name="followPosition">The desired follow position.</param>
        /// <param name="offset">The specified offset.</param>
        /// <returns></returns>

        private Vector3 ApplyOffsetToFollowPosition(Vector3 followPosition, Vector3 offset)
        {


            Vector3 offsetedPosition = followPosition + offset;
            Vector3 distance = followPosition - offsetedPosition;
            desiredFollowDirection = distance.normalized;

            // check if there are any obstructions along the way.

            if (UnityEngine.Physics.SphereCast(
                followPosition,
                CameraRadius,
                -desiredFollowDirection,
                out RaycastHit hit,
                distance.magnitude,
                obstructionLayer,
                QueryTriggerInteraction.Ignore
            ))
            {

                // apply a 'bubble' around the camera so it doesnt clip into walls.

                return hit.point + (desiredFollowDirection * CameraRadius) + (hit.normal * CameraRadius);
            }
            else
            {
                return offsetedPosition; // apply the player camera input as positional offset.
            }
        }

        /// <summary>
        /// Calculates a position in world space from the camera transform position to a desired world position;
        /// smoothly moving between the two positions.
        /// </summary>
        /// <param name="desiredWorldPosition"></param>
        /// <returns></returns>

        private Vector3 CalculateDeltaPosition(Vector3 desiredWorldPosition)
        {

            Vector3 deltaPosition;

            if (lockOnTarget != null)
            {
                deltaPosition = Vector3.Slerp(transform.position, desiredWorldPosition, LockOnTargetSmoothSpeed * UnityEngine.Time.deltaTime);
            }
            else
            {
                deltaPosition = Vector3.MoveTowards(transform.position, desiredWorldPosition, MouseInputSmoothSpeed * UnityEngine.Time.deltaTime);
            }

            // if we are currently not shaking.

            return deltaPosition;
        }


        /// 
        /// Rotational Updating.
        /// 


        /// <summary>
        /// Calculates a rotation for the camera to look at the follow target or lock on target (if available)
        /// </summary>
        /// <returns>A Quaternion that looks at the relevant target.</returns>

        private Quaternion CalculateRotationToRelativeTarget()
        {

            // look at the direction that the camera is pointing in relation to players input
            // rather than tracking the target so the camera doesnt bug out when the target is
            // too close to the player; as well as making sure that shaking the camera doesnt
            // track the player.

            if (lockOnTarget != null)
            {

                // smoothly lerp to lock on target.

                return
                    Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(desiredFollowDirection, Vector3.up),
                        LookAtLockOnTargetSmoothSpeed * UnityEngine.Time.deltaTime
                    );
            }
            else
            {

                // snap to input for responsiveness.

                return Quaternion.LookRotation(desiredFollowDirection, Vector3.up);
            }
        }

        /// <summary>
        /// Invokes the OnRotated Callback if the camera has been rotated since the last time this was called.
        /// </summary>

        private void CheckIfRotated()
        {
            if (transform.rotation.Equals(previousRotation) == false)
            {
                Rotated?.Invoke();
            }
            previousRotation = transform.rotation;
        }

        /// <summary>
        /// Increments the current look rotation. 
        /// </summary>
        /// <param name="deltaMovement">The amount of movement to apply.</param>

        private void ApplyDeltaMovementToLookRotation(Vector2 deltaMovement)
        {
            deltaMovement *= inputSensitivity * UnityEngine.Time.deltaTime;
            Vector3 eulerAngles = lookRotation.eulerAngles;

            // Convert pitch (x) to -180..180 range
            // as Unity never gives angles from -180..180,
            // always 0-360.

            float pitch = eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            // Apply delta and clamp

            pitch = Mathf.Clamp(pitch - deltaMovement.y, LowerPitchLimit, UpperPitchLimit);

            // apply yaw.

            float yaw = eulerAngles.y + deltaMovement.x;

            // set the new rotation.

            lookRotation = Quaternion.Euler(pitch, yaw, 0f);
        }


        ///
        /// Shaker Functions.
        /// 


        /// <summary>
        /// Calculates a position in world space from the shaker transform position to the desired shaker position;
        /// smoothly moving between the two positions.
        /// </summary>
        /// <param name="desiredPosition">The position for the shaker to move towards (in local space).</param>
        /// <returns></returns>

        private Vector3 CalculateShakerDeltaLocalPosition(Vector3 desiredPosition)
        {

            // smoothly update position instead of snapping

            Vector3 deltaPosition = Vector3.MoveTowards(shaker.localPosition, desiredPosition, UnityEngine.Time.deltaTime * shakeStrength);

            // short-circuit if we have returned to default state.

            if (deltaPosition == Vector3.zero && shaker.localPosition == Vector3.zero)
                return Vector3.zero;

            // clamp shake strength so it doesn't overshoot

            deltaPosition.x = Mathf.Clamp(deltaPosition.x, -shakeStrength, shakeStrength);
            deltaPosition.y = Mathf.Clamp(deltaPosition.y, -shakeStrength, shakeStrength);
            deltaPosition.z = Mathf.Clamp(deltaPosition.z, -shakeStrength, shakeStrength);

            // convert to world space

            Vector3 nextShakerWorldPos = transform.TransformPoint(deltaPosition);
            Vector3 worldDir = (nextShakerWorldPos - transform.position).normalized;
            float worldDist = Vector3.Distance(transform.position, nextShakerWorldPos);

            // check for obstruction
            if (UnityEngine.Physics.SphereCast(
                transform.position,
                CameraRadius,
                worldDir,
                out RaycastHit hit,
                worldDist,
                obstructionLayer,
                QueryTriggerInteraction.Ignore
            ))
            {

                // push the shaker out so it doesn’t clip into walls
                Vector3 correctedWorldPos = hit.point - (worldDir * CameraRadius) + (hit.normal * CameraRadius);
                return transform.InverseTransformPoint(correctedWorldPos);
            }

            return deltaPosition;
        }

        /// <summary>
        /// Starts to shake this camera indefinently.
        /// </summary>
        /// <param name="strength">The specified strength.</param>

        public void StartShaking(float strength)
        {
            Shake = ShakeFixedUpdate;
            shakeStrength = strength;
            shakeTimer.Loop = true;
            shakeTimer.Begin(int.MaxValue); // max value so it doesnt call loop increbily often.
        }

        /// <summary>
        /// Starts to shake this camera.
        /// </summary>
        /// <param name="strength">The specified strength.</param>
        /// <param name="time">The amount of time in seconds (scaled time) to shake.</param>

        public void StartShaking(float strength, float time)
        {
            Shake = ShakeFixedUpdate;
            shakeStrength = strength;
            shakeTimer.Loop = false;
            shakeTimer.Begin(time);
        }

        /// <summary>
        /// Stops shaking the camera if it is currently shaking.
        /// </summary>

        private void StopShaking()
        {
            shakeTimer.Halt();
            Shake = null;
            targetShakerOffset = Vector3.zero;
        }

        /// <summary>
        /// Assign a random position within the shake strengths range for the shaker to move to.
        /// </summary>

        private void ShakeFixedUpdate()
        {
            targetShakerOffset = new Vector3(
                UnityEngine.Random.Range(-shakeStrength, shakeStrength),
                UnityEngine.Random.Range(-shakeStrength, shakeStrength),
                UnityEngine.Random.Range(-shakeStrength, shakeStrength)
            );
        }


        ///
        /// Input Sensitivity Handling.
        /// 


        /// <summary>
        /// Sets the input sensitivity of this camera to MouseSensitivity if the current input is a keyboard
        /// and to GamepadSensitivity if the current input device is a gamepad.
        /// </summary>

        private void InitialiseInputSensitivity()
        {
            if (InputManager.Singleton.InputDeviceType == Input.InputDeviceType.KeyboardAndMouse)
            {
                inputSensitivity = MouseSensitivity;
            }
            else
            {
                inputSensitivity = JoystickSensitivity;
            }
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents()
        {
            LinkTimerEvents();
            LinkInputEvents();
            LinkLockOnTargetDetectorEvents();
        }


        private void UnlinkEvents()
        {
            UnlinkTimerEvents();
            UnlinkInputEvents();
            UnlinkLockOnTargetDetectorEvents();
        }


        ///
        /// Lock On Target Detector Linkage.
        /// 


        private void LinkLockOnTargetDetectorEvents()
        {
            lockOnTargetDetector.TargetLeftRange += OnLockOnTargetLeftRange;
        }

        private void UnlinkLockOnTargetDetectorEvents()
        {
            lockOnTargetDetector.TargetLeftRange -= OnLockOnTargetLeftRange;
        }

        private void OnLockOnTargetLeftRange()
        {
            SetLockOnTarget(null);
        }


        ///
        /// Timer Linkage.
        /// 


        private void LinkTimerEvents()
        {
            shakeTimer.Halted += OnShakeTimerHalted;
        }

        private void UnlinkTimerEvents()
        {
            shakeTimer.Halted -= OnShakeTimerHalted;
        }

        private void OnShakeTimerHalted()
        {
            StopShaking();
        }


        /// 
        /// Input Linkage.
        /// 


        private void LinkInputEvents()
        {

            InputManager input = InputManager.Singleton;

            input.LockOnToggle += OnLockOnToggle;
            input.LockOnPrevious += OnLockOnPrevious;
            input.LockOnNext += OnLockOnNext;
            input.InputDeviceTypeSet += OnInputDeviceTypeSet;
        }

        private void UnlinkInputEvents()
        {

            InputManager input = InputManager.Singleton;

            input.LockOnToggle -= OnLockOnToggle;
            input.LockOnPrevious -= OnLockOnPrevious;
            input.LockOnNext -= OnLockOnNext;
            input.InputDeviceTypeSet -= OnInputDeviceTypeSet;
        }

        private void OnLockOnToggle()
        {
            if (lockOnTarget == null)
            {

                // get a new target in front of the camera.

                Transform newLockOnTarget = lockOnTargetDetector.GetTarget(transform.forward);

                if (newLockOnTarget != null)
                {

                    // if a target was found: lock onto it.
                    SetLockOnTarget(newLockOnTarget);
                }
            }
            else
            {

                // stop locking onto a target if we are tracking one.
                SetLockOnTarget(null);
            }
        }

        private void OnLockOnPrevious()
        {

            // if we are currently locked onto a target.

            if (lockOnTarget != null)
            {
                Transform previousLockOnTarget = lockOnTargetDetector.GetPrevious();
                if (previousLockOnTarget != null)
                {
                    SetLockOnTarget(previousLockOnTarget);
                }
            }
        }

        private void OnLockOnNext()
        {

            // if we are currently locked onto a target.

            if (lockOnTarget != null)
            {
                Transform nextLockOnTarget = lockOnTargetDetector.GetNext();
                if (nextLockOnTarget != null)
                {
                    SetLockOnTarget(nextLockOnTarget);
                }
            }
        }

        private void OnInputDeviceTypeSet(Input.InputDeviceType inputDeviceType)
        {
            InitialiseInputSensitivity();
        }
    }

}

