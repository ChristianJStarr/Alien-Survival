using MLAPI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Player Manager Script for Primary Scene. 
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// Network Object on Player.
    /// </summary>
    private NetworkedObject networkedObject;

    /// <summary>
    /// Player Manager Start Function.
    /// </summary>
    private void Start()
    {
        networkedObject = GetComponent<NetworkedObject>();
        //Check if object belongs to this player.
        if (networkedObject.IsLocalPlayer) 
        {
            LocalPlayerStart();        
        }
        else 
        {
            NetPlayerStart();
        }
    }
    /// <summary>
    /// Setup this Player gameobject as a local player.
    /// </summary>
    private void LocalPlayerStart() 
    {
        //Do local player things.
    }
    /// <summary>
    /// Setup this Player gameobject as a network player.
    /// </summary>
    private void NetPlayerStart() 
    {
        GameObject cam = GetComponentInChildren<Camera>().gameObject;
        FirstPersonController fps = GetComponentInChildren<FirstPersonController>();
        if (cam != null) { Destroy(cam); }
        if (fps != null) { Destroy(fps); }
    }
}
