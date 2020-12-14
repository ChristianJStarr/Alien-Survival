using MLAPI;
using MLAPI.Serialization.Pooled;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCommandManager : MonoBehaviour
{
    public Settings settings;
    private GameServer gameServer;
    private PlayerCommand command;
    public PlayerCameraManager playerCameraManager;
    private PlayerControlObject controlObject;
    private NetworkingManager networkingManager;
    
    //Animate
    private Vector3 lastPosition;
    private Vector2 lastLook;

    //Selected Inventory Hotbar Slot
    public int selectedSlot;
    public int lastSelectedSlot;

    //Counter for Sending Correction 
    public int sendCorrectionAmount = 20; //Amount of frames until correction is sent
    private int currentFrame = 0;


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

    private void Update() 
    {
        UpdatePlayerCommand(); //update player command values
    }

    void FixedUpdate() //50 times per second.
    {
        RotatePlayerLocally();
        SendPlayerCommand();
        currentFrame++;
    }

    //Attempt to send Player Command to Server
    private void SendPlayerCommand()
    {
        if (networkingManager == null) return;
        if(CommandIsValuable()) //Is this command valuable enough to send
        {
            lastLook = command.look;
            MovePlayerLocally(); //Move Player
            AnimatePlayerLocally(); //Animate Player
            lastSelectedSlot = selectedSlot; //Set last selected slot
            using (PooledBitStream writeStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
                {
                    //CommandBreakdown
                    writer.WriteSinglePacked(command.networkTime);
                    writer.WriteVector2Packed(command.move);
                    writer.WriteVector2Packed(command.look);
                    writer.WriteBool(command.correction);
                    if (command.correction)
                    {
                        writer.WriteVector3Packed(command.correction_position);
                    }
                    writer.WriteBool(command.jump);
                    writer.WriteBool(command.crouch);
                    writer.WriteBool(command.use);
                    writer.WriteBool(command.reload);
                    writer.WriteBool(command.aim);
                    writer.WriteInt16Packed((short)command.selectedSlot);
                    gameServer.ClientSendPlayerCommand(writeStream);
                }
            }
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
        if (currentFrame == sendCorrectionAmount)
        {
            currentFrame = 0;
            command.correction = true;
            command.correction_position = controlObject.transform.position;
        }
        command.look.y = controlObject.transform.rotation.eulerAngles.y;
        command.look.x = controlObject.cameraObject.localRotation.eulerAngles.x;
        command.jump = CrossPlatformInputManager.GetButton("Jump");
        command.crouch = CrossPlatformInputManager.GetButton("Crouch");
        command.use = CrossPlatformInputManager.GetButton("Use");
        command.reload = CrossPlatformInputManager.GetButtonDown("Reload");
        if (CrossPlatformInputManager.GetButtonDown("Aim")) 
        {
            command.aim = !command.aim;
        }
        command.selectedSlot = selectedSlot;

        DebugMenu.UpdateCommand(command);
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

        Quaternion m_CameraTargetRot = ClampRotationAroundXAxis(controlObject.cameraObject.localRotation * Quaternion.Euler(-lookAxis.x, 0f, 0f)); //Clamp This
        if (controlObject.cameraObject.localRotation != m_CameraTargetRot)
        {
            //controlObject.cameraObject.localRotation = m_CameraTargetRot;
            controlObject.cameraObject.localRotation = Quaternion.Lerp(controlObject.cameraObject.localRotation, m_CameraTargetRot, 3 * Time.fixedDeltaTime);
        }
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.z /= q.w;
        q.y /= q.w;
        q.w = 1.0F;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -70, 70);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q.normalized;
    }

    //Animate the Player Locally
    private void AnimatePlayerLocally() 
    {
        if (controlObject == null) return;
        //Animate
        controlObject.Animate(GetAnimationVector(controlObject.transform, lastPosition));
        lastPosition = controlObject.transform.position;
    }

    //Is the current command valuable enough for send?
    private bool CommandIsValuable()
    {
        return (command.look != lastLook || command.move.magnitude > 0 || command.jump || command.crouch || command.use || command.selectedSlot != lastSelectedSlot);
    }

    
    public static Vector2 GetAnimationVector(Transform current, Vector3 previous) 
    {
        //Animate
        Vector3 distance = (current.position - previous);
        if (distance != Vector3.zero)
        {
            distance /= Time.deltaTime;
            distance = current.InverseTransformDirection(distance);
            distance.x = Mathf.Clamp(distance.x, -1, 2);
            distance.z = Mathf.Clamp(distance.z, -1, 1);
        }
        return new Vector2(distance.x, distance.z);
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

    public bool correction = false;
    public Vector3 correction_position;

    //Buttons
    public bool jump;
    public bool crouch;
    public bool use;
    public bool reload;
    public bool aim;

    //Hotbar
    public int selectedSlot;

}


