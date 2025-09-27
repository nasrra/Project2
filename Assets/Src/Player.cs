using UnityEngine;

public class Player : MonoBehaviour{

    [Header("Components")]
    [SerializeField] private Movement movement;
    [SerializeField] private JumpMovement jumpMovement;
    [SerializeField] private CameraController cam;



    /// 
    /// Base.
    /// 


    private void OnEnable(){
        Link();
    }

    private void OnDisable(){
        Unlink();
    }


    /// 
    /// Movement.
    /// 


    private void UpdateMoveDirection(Vector3 moveDirection){
        Vector3 cameraForwardXZ = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        Vector3 cameraRightXZ   = new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
        movement.moveDirection  = moveDirection.x * cameraRightXZ + moveDirection.y * cameraForwardXZ;
    }
    

    /// 
    /// Linkage.
    /// 


    private void Link(){
        LinkCamera();
        LinkInput();
    }

    private void Unlink(){
        UnlinkCamera();
        UnlinkInput();
    }


    ///
    /// Camera Linkage.
    /// 

    
    private void LinkCamera(){
        cam.Rotated += OnCameraRotated;
    }

    private void UnlinkCamera(){
        cam.Rotated -= OnCameraRotated;
    }

    private void OnCameraRotated(){
        UpdateMoveDirection(InputManager.Singleton.moveInput);
    }


    /// 
    /// Input Linkage.
    /// 


    private void LinkInput(){
        
        InputManager input = InputManager.Singleton; 
        
        input.Move      += OnMoveInput;
        input.JumpStart += OnJumpStartInput;
        input.JumpStop  += OnJumpStopInput;
    }

    private void UnlinkInput(){
        
        InputManager input = InputManager.Singleton; 
        
        input.Move      += OnMoveInput;
        input.JumpStart += OnJumpStartInput;
        input.JumpStop  += OnJumpStopInput;
    }

    private void OnMoveInput(Vector2 moveInput){
        UpdateMoveDirection(moveInput);
    }

    private void OnJumpStartInput(){
        jumpMovement.StartJumping();
    }

    private void OnJumpStopInput(){
        jumpMovement.StopJumping();
    }
}
