using MLAPI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

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
                SelectedItemHandler selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
                CharacterController characterController = GetComponent<CharacterController>();

                if (networkedObject != null)
                {
                    if (networkedObject.IsLocalPlayer)
                    {
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
                Destroy(playerCamera);
                Destroy(playerViewCamera);
            }
        }
    }
}
