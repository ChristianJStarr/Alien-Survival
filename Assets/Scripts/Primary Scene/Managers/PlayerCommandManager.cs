using MLAPI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCommandManager : MonoBehaviour
{
    public Settings settings;
    private GameServer gameServer;
    private PlayerCommand command;
    private PlayerControlObject controlObject;
    
    //Animate
    private Vector3 lastPosition;

    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer)
        {
            Destroy(this);
        }
        gameServer = GameServer.singleton;
        command = new PlayerCommand(){sensitivity = settings.sensitivity};
    }

    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;
    }

    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }

    public void Register(PlayerControlObject _controlObject) 
    {
        controlObject = _controlObject;
        lastPosition = controlObject.transform.position;
    }

    public void Change() 
    {
        command.sensitivity = settings.sensitivity;
    }

    void FixedUpdate() //60 times per second.
    {
        SendPlayerCommand();
        
    }



    private void SendPlayerCommand()
    {
        bool send = false;
        command.move.x = CrossPlatformInputManager.GetAxis("Horizontal");
        command.move.y = CrossPlatformInputManager.GetAxis("Vertical");
        command.look.y = CrossPlatformInputManager.GetAxis("Mouse X");
        command.look.x = CrossPlatformInputManager.GetAxis("Mouse Y");
        command.jump = CrossPlatformInputManager.GetButton("Jump");
        command.crouch = CrossPlatformInputManager.GetButton("Crouch");

        if(controlObject != null) 
        {
            //Move
            if(command.move.magnitude > 0 || command.jump || command.crouch) 
            {
                controlObject.Move(command.move, command.jump, command.crouch);
                send = true;
            }
            //Look
            if(command.look.magnitude > 0) 
            {
                controlObject.Rotate(command.look, command.sensitivity);
                send = true;
            }
            //Animate
            if (controlObject.transform.position != lastPosition)
            {
                Vector3 velocity = (controlObject.transform.position - lastPosition) / Time.fixedDeltaTime;
                if (velocity.x != 0) //forward speed
                {
                    float animateX = Mathf.Clamp(velocity.x, -1, 2);
                    controlObject.animator.SetFloat("horizontal", animateX);
                }
                else 
                {
                    controlObject.animator.SetFloat("horizontal", 0);
                }
                if (velocity.z != 0) //side speed
                {
                    float animateZ = Mathf.Clamp(velocity.z, -1, 1);
                    controlObject.animator.SetFloat("vertical", animateZ);
                }
                else
                {
                    controlObject.animator.SetFloat("vertical", 0);
                }
            }
            else 
            {
                controlObject.animator.SetFloat("horizontal", 0);
                controlObject.animator.SetFloat("vertical", 0);
            }
            lastPosition = controlObject.transform.position;
        }

        if (send) 
        {
            gameServer.ClientSendPlayerCommand(command);
        }
    }
}

public class PlayerCommand 
{
    //Client
    public ulong clientId;
    public float networkTime;

    //Move Axis
    public Vector2 move;
    //Look Axis
    public Vector2 look;
    //Look Sensitivity
    public Vector2 sensitivity;

    //Buttons
    public bool jump;
    public bool crouch;

    
}
