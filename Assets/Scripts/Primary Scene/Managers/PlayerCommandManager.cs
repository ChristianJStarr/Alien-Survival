using MLAPI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCommandManager : MonoBehaviour
{
#if (!UNITY_SEVER)
    public Settings settings;
    private GameServer gameServer;
    public PlayerCameraManager playerCameraManager;
    private PlayerControlObject controlObject;
    private NetworkingManager networkingManager;

    //Animate
    private Vector3 lastPosition;
    private Vector2 lastLook;

    public int selectedSlot;
    public int lastSelectedSlot;

    public ClientCommand[] commands = new ClientCommand[4];


    private void Start()
    {
        gameServer = GameServer.singleton;
        networkingManager = NetworkingManager.Singleton;
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR
        if (networkingManager.IsClient) 
        {
            SendPlayerCommand();
        }
#elif !UNITY_SERVER
        SendPlayerCommand();
#endif
    }

    private void SendPlayerCommand()
    {
        commands[3] = commands[2];
        commands[2] = commands[1];
        commands[1] = commands[0];
        commands[0] = GenerateClientCommand();

        lastSelectedSlot = commands[0].selected_slot;
        lastLook = commands[0].look_axis;

        MovePlayerLocally(commands[0]);
        RotatePlayerLocally(commands[0]);
        AnimatePlayerLocally(commands[0]);

        gameServer.ClientSendPlayerCommand(BitHelper.ConvertClientCommand(commands));
    }
    private ClientCommand GenerateClientCommand() 
    {
        return new ClientCommand()
        {
            tick = ServerTick.GetTick(networkingManager.NetworkTime),
            move_axis = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical")),
            look_axis = GetLookAxis(),
            jump = CrossPlatformInputManager.GetButton("Jump"),
            crouch = CrossPlatformInputManager.GetButton("Crouch"),
            use = CrossPlatformInputManager.GetButton("Use"),
            reload = CrossPlatformInputManager.GetButtonDown("Reload"),
            selected_slot = selectedSlot,
            aim = CrossPlatformInputManager.GetButtonDown("Aim")
        };
    }

    private void MovePlayerLocally(ClientCommand command) 
    {
        if (controlObject == null) controlObject = LocalPlayerControlObject.GetLocalPlayer();
        if (controlObject && (command.move_axis.magnitude > 0 || command.jump || command.crouch))
        {
            controlObject.Move(command.move_axis, command.jump, command.crouch);
        }
    }
    private void RotatePlayerLocally(ClientCommand command) 
    {
        if (controlObject == null) controlObject = LocalPlayerControlObject.GetLocalPlayer();
        if (controlObject && command.look_axis.magnitude > 0) 
        {
            Debug.Log("Rotating " + command.look_axis.ToString());
            controlObject.Rotate(command.look_axis);
        }
    }
    private void AnimatePlayerLocally(ClientCommand command) 
    {
        if (controlObject == null) controlObject = LocalPlayerControlObject.GetLocalPlayer();
        if (controlObject)
        {
            controlObject.Animate(GetAnimationVector(controlObject.transform, lastPosition));
            lastPosition = controlObject.transform.position;
        }
    }
    
    public Vector2 GetLookAxis() 
    {
        settings.sensitivity.x = Mathf.Clamp(settings.sensitivity.x, 0.1F, 1);
        settings.sensitivity.y = Mathf.Clamp(settings.sensitivity.y, 0.1F, 1);
        return new Vector2(CrossPlatformInputManager.GetAxis("Mouse Y"), CrossPlatformInputManager.GetAxis("Mouse X")) * settings.sensitivity * 8; //apply sensitivity
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
    
#endif
    }

public struct ClientCommand 
{
    public ulong clientId;
    public int tick;
    public int selected_slot;
    public Vector2 move_axis;
    public Vector2 look_axis;
    public Vector3 move_sync;
    public bool jump;
    public bool crouch;
    public bool use;
    public bool reload;
    public bool aim;
}


