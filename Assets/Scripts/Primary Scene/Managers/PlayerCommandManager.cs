using MLAPI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCommandManager : MonoBehaviour
{
    public int sendsPerSecond;
    private GameServer gameServer;
    private float lastSendTime;
    private string authkey;

    private void Start() 
    {
        if (NetworkingManager.Singleton.IsServer)
            Destroy(this);


        authkey = PlayerPrefs.GetString("authkey");
        gameServer = GameServer.singleton;
    }

    void FixedUpdate()
    {
        if (NetworkingManager.Singleton.NetworkTime - lastSendTime >= (1f / sendsPerSecond))
        {
            SendPlayerCommand();
        }
    }

    private void SendPlayerCommand() 
    {
        gameServer.ClientSendPlayerCommand(authkey, new PlayerCommand()
        {
            moveX = CrossPlatformInputManager.GetAxis("Horizontal"),
            moveY = CrossPlatformInputManager.GetAxis("Vertical"),
            lookX = CrossPlatformInputManager.GetAxis("TouchHorizontal"),
            lookY = CrossPlatformInputManager.GetAxis("TouchVertical"),

            jump = CrossPlatformInputManager.GetButton("Jump"),
            crouch = CrossPlatformInputManager.GetButton("Crouch")
        });
    }
}

public class PlayerCommand 
{
    //Client
    public ulong clientId;

    //Move Axis
    public float moveX;
    public float moveY;

    //Look Axis
    public float lookX;
    public float lookY;
    public float sensitivityX;
    public float sensitivityY;

    //Buttons
    public bool jump;
    public bool crouch;

}
