﻿using MLAPI;
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
    public GameObject playerViewCamera;



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
                CharacterController characterController = GetComponent<CharacterController>();

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
                        InventoryGfx inventoryGfx = FindObjectOfType<InventoryGfx>();
                        if (inventoryGfx != null) 
                        {
                            inventoryGfx.playerViewCamera = playerViewCamera;
                        }
                    }

                    else
                    {
                        if (firstPerson != null)
                        {
                            Destroy(firstPerson);
                        }
                        if(characterController != null) 
                        {
                            Destroy(characterController);
                        }

                        Destroy(playerCamera);
                        Destroy(playerViewCamera);
                    }
                }
            }
            else
            {
                FirstPersonController firstPerson = GetComponent<FirstPersonController>();
                CharacterController characterController = GetComponent<CharacterController>();

                if (firstPerson != null)
                {
                    Destroy(firstPerson);
                }
                if (characterController != null)
                {
                    Destroy(characterController);
                }

                Destroy(playerCamera);
                Destroy(playerViewCamera);
            }
        }
        else 
        {
            //No Networking Manager Detected. 
        }
    }
}
