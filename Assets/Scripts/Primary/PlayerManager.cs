using Cinemachine;
using MLAPI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using BreadcrumbAi;
/// <summary>
/// Player Manager Script for Primary Scene. 
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public Transform handAnchor;
    public GameObject playerCamera;
    private void Start()
    {
        if (NetworkingManager.Singleton != null)
        {
            if (NetworkingManager.Singleton.IsClient)
            {
                NetworkedObject networkedObject = GetComponent<NetworkedObject>();
                FirstPersonController firstPerson = GetComponent<FirstPersonController>();
                Breadcrumbs breadcrumb = GetComponent<Breadcrumbs>();
                SelectedItemHandler selectedItemHandler = FindObjectOfType<SelectedItemHandler>();


                if (networkedObject != null)
                {
                    if (networkedObject.IsLocalPlayer)
                    {
                        if (breadcrumb != null)
                        {
                            Destroy(breadcrumb);
                        }
                        if(selectedItemHandler != null) 
                        {
                            selectedItemHandler.animator = GetComponent<Animator>();
                            selectedItemHandler.handAnchor = handAnchor;
                        }
                    }
                    else
                    {
                        if (firstPerson != null)
                        {
                            Destroy(firstPerson);
                        }
                        Destroy(playerCamera);
                    }
                }
            }
            else
            {
                FirstPersonController firstPerson = GetComponent<FirstPersonController>();
                if (firstPerson != null)
                {
                    Destroy(firstPerson);
                }
                Destroy(playerCamera);
            }
        }
        else 
        {
            //No Networking Manager Detected. 
        }
    }
}
