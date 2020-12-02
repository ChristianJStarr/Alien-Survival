using MLAPI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCommandManager : MonoBehaviour
{
    public Settings settings;
    private GameServer gameServer;
    private PlayerCommand command;
    private PlayerControlObject controlObject;
    private NetworkingManager networkingManager;
    
    //Animate
    private Vector3 lastPosition;
    private Vector2 lastLook;

    //Selected Inventory Hotbar Slot
    public int selectedSlot;
    public int lastSelectedSlot;


    private void Start()
    {
        gameServer = GameServer.singleton;
        command = new PlayerCommand();
        if (NetworkingManager.Singleton != null)
        {
            networkingManager = NetworkingManager.Singleton;
            if (networkingManager.IsServer) Destroy(this);
        }
    }

    public void Register(PlayerControlObject _controlObject) 
    {
        controlObject = _controlObject;
        lastPosition = controlObject.transform.position;
    }

    void FixedUpdate() //60 times per second.
    {
        RotatePlayerLocally();
        SendPlayerCommand();
    }

    //Attempt to send Player Command to Server
    private void SendPlayerCommand()
    {
        if (networkingManager == null) return;
        UpdatePlayerCommand(); //update player command values
        if(CommandIsValuable()) //Is this command valuable enough to send
        {
            lastLook = command.look;
            MovePlayerLocally(); //Move Player
            AnimatePlayerLocally(); //Animate Player
            lastSelectedSlot = selectedSlot; //Set last selected slot
            gameServer.ClientSendPlayerCommand(command); //Send out this command
        }
        else if (controlObject != null)
        {
            controlObject.Animate(Vector2.zero);
        }
    }

    //Update Player Command Values to Recent
    private void UpdatePlayerCommand()
    {
        command.networkTime = networkingManager.NetworkTime;
        command.move.x = CrossPlatformInputManager.GetAxis("Horizontal");
        command.move.y = CrossPlatformInputManager.GetAxis("Vertical");
        command.look.y = controlObject.transform.rotation.eulerAngles.y;
        command.look.x = controlObject.cameraObject.localRotation.eulerAngles.x;
        command.jump = CrossPlatformInputManager.GetButton("Jump");
        command.crouch = CrossPlatformInputManager.GetButton("Crouch");
        command.use = CrossPlatformInputManager.GetButton("Use");
        command.selectedSlot = selectedSlot;
    }

    //Move the Player Locally
    private void MovePlayerLocally() 
    {
        if (controlObject == null) return;
        //Move
        if (command.move.magnitude > 0 || command.jump || command.crouch)
        {
            controlObject.Move(command.move, command.jump, command.crouch);
        }
    }

    private void RotatePlayerLocally() 
    {
        if (controlObject == null) return;
        settings.sensitivity.x = Mathf.Clamp(settings.sensitivity.x, 0.1F, 1);
        settings.sensitivity.y = Mathf.Clamp(settings.sensitivity.y, 0.1F, 1);
        Vector2 lookAxis = new Vector2(CrossPlatformInputManager.GetAxis("Mouse Y"), CrossPlatformInputManager.GetAxis("Mouse X"));
        lookAxis *= settings.sensitivity * 8; //apply sensitivity

        //Player Object ( X Axis )
        Quaternion m_CharacterTargetRot = controlObject.transform.rotation * Quaternion.Euler(0f, lookAxis.y, 0f);
        if (controlObject.transform.rotation != m_CharacterTargetRot)
        {
            controlObject.transform.rotation = Quaternion.Slerp(controlObject.transform.rotation, m_CharacterTargetRot, 3 * Time.fixedDeltaTime);
        }

        //Camera Object ( Y Axis )
        Quaternion m_CameraTargetRot = controlObject.cameraObject.localRotation * Quaternion.Euler(-lookAxis.x, 0f, 0f); //Clamp This
        if (controlObject.cameraObject.localRotation != m_CameraTargetRot)
        {
            controlObject.cameraObject.localRotation = Quaternion.Slerp(controlObject.cameraObject.localRotation, m_CameraTargetRot, 3 * Time.fixedDeltaTime);
        }
    }

    //Animate the Player Locally
    private void AnimatePlayerLocally() 
    {
        if (controlObject == null) return;
        //Animate
        Vector3 distance = (controlObject.transform.position - lastPosition);
        if (distance != Vector3.zero) 
        {
            distance /= Time.deltaTime;
            distance = controlObject.transform.InverseTransformDirection(distance);
            distance.x = Mathf.Clamp(distance.x, -1, 2);
            distance.z = Mathf.Clamp(distance.z, -1, 1);
            controlObject.Animate(new Vector2(distance.x, distance.z));
            lastPosition = controlObject.transform.position;
        }
        else 
        {
            controlObject.Animate(Vector2.zero);
        }
    }

    //Is the current command valuable enough for send?
    private bool CommandIsValuable()
    {
        return (command.look != lastLook || command.move.magnitude > 0 || command.jump || command.crouch || command.use || command.selectedSlot != lastSelectedSlot);
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

    //Buttons
    public bool jump;
    public bool crouch;
    public bool use;

    //Hotbar
    public int selectedSlot;

}


