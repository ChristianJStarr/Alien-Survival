using MLAPI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Player Manager Script for Primary Scene. 
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// Player Manager Start Function.
    /// </summary>
    private void Start()
    {
        //Check if object belongs to this player.
        if (GetComponent<NetworkedObject>().IsLocalPlayer) 
        {       
        }
        else 
        {
            GameObject cam = GetComponentInChildren<Camera>().gameObject;
            FirstPersonController fps = GetComponentInChildren<FirstPersonController>();
            if (cam != null) { Destroy(cam); }
            if (fps != null) { Destroy(fps); }
        }
        if (NetworkingManager.Singleton.IsClient) 
        {
            BreadcrumbAi.Breadcrumbs bc = GetComponentInChildren<BreadcrumbAi.Breadcrumbs>();
            if (bc != null) { Destroy(bc); }
        }
    }

}
