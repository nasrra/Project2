using UnityEngine;

[RequireComponent(typeof(Movement))]
public class JumpMovement : MonoBehaviour{
    [Header("Components")]
    [SerializeField] private Movement movement;

    [Header("Data")]
    private Vector3 initialJumpVelocity;
    private Vector3 jumpVelocity;
    
    [SerializeField] private float jumpSpeed;
    public float JumpSpeed => jumpSpeed;
    
    [SerializeField] private float jumpDecay;
    public float JumpDecay => jumpDecay;
    
    private bool isJumping = false;

    private void Awake(){
        UpdateInitialJumpVelocity();
    }

    private void Update(){
        HandleJumping();
    }

    public void SetJumpSpeed(float jumpSpeed){
        this.jumpSpeed = jumpSpeed;
        UpdateInitialJumpVelocity();
    }

    public void SetJumpDecay(float jumpDecay){
        this.jumpDecay = jumpDecay;
    }

    public void UpdateInitialJumpVelocity(){
        initialJumpVelocity = jumpSpeed * Vector3.up;  
    }

    public void StartJumping(){
        jumpVelocity = initialJumpVelocity;
        isJumping = true;
    }

    public void HandleJumping(){
        if(isJumping == true &&jumpVelocity.sqrMagnitude > 0){
            movement.AddOneFrameVelocity(jumpVelocity);
            jumpVelocity = Vector3.MoveTowards(jumpVelocity, Vector3.zero, jumpDecay * Time.deltaTime);
        }
    }

    public void StopJumping(){
        jumpVelocity = Vector3.zero;
        isJumping = false;
    }
}
