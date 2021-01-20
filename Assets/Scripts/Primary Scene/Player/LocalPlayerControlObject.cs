using UnityEngine;

public class LocalPlayerControlObject : MonoBehaviour
{
    public static LocalPlayerControlObject Singleton;
    private PlayerControlObject player;

    private void Awake() 
    {
        Singleton = this;
    }

    //Get Local Player Control Object
    public static PlayerControlObject GetLocalPlayer() 
    {
        if (Singleton) 
        {
            return Singleton.GetPlayer();
        }
        return null;
    }
    private PlayerControlObject GetPlayer() 
    {
        return player;
    }


    //Get Local Player Transform
    public static Transform GetLocalPlayerTransform()
    {
        if (Singleton)
        {
            return Singleton.GetPlayerTransform();
        }
        return null;
    }
    private Transform GetPlayerTransform()
    {
        if (player)
        {
            return player.transform;
        }
        return null;
    }


    //Set Local Player Control Object
    public static void SetLocalPlayer(PlayerControlObject _player)
    {
        if (Singleton) 
        {
            Singleton.SetPlayer(_player);
        }
    }
    private void SetPlayer(PlayerControlObject _player) 
    {
        Debug.Log(string.Format("Local Player Object Set. Object: {0}", _player.OwnerClientId));
        player = _player;
    }
}
