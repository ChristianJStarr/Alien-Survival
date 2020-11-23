using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerLocalMovement : MonoBehaviour
{
    public Settings settings;

    public float stickToGroundForce = 5;
    public float jumpSpeed = 1;
    public float gravityMultiplier = 1;

    public float lookMaximumX = 20;
    public float lookMinimumX = 90;

    private PlayerControlObject controlObject;

    public void Register(PlayerControlObject playerControlObject) 
    {
        settings = Resources.Load("Data/GameSettings") as Settings;
        controlObject = playerControlObject;
    }

    private void FixedUpdate() 
    {
        if(controlObject != null) 
        {
            RotatePlayer(); // Rotate Player
            MovePlayer(); // Move Character
        }
    }

    private void RotatePlayer() 
    {
        Vector2 look = new Vector2(CrossPlatformInputManager.GetAxis("Mouse Y"), CrossPlatformInputManager.GetAxis("Mouse X"));
        if(look.magnitude > 0) 
        {
            controlObject.Rotate(look, settings.sensitivity);
        }
    }

    private void MovePlayer() 
    {
        bool jump = CrossPlatformInputManager.GetButton("Jump");
        bool crouch = CrossPlatformInputManager.GetButton("Crouch");
        Vector2 move = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));
        
        if(move.magnitude > 0 || jump || crouch) 
        {
            controlObject.Move(move, jump, crouch);
        }
    }
}
