using UnityEngine;
using TMPro;
using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class Reticle : MonoBehaviour
{
    private RectTransform reticle;
    private int regularLayerMask, builderLayerMask;
    private Vector3 pos;
    private Vector3 pos2;
    public GameObject reticleTip, reticleNotify;
    public TextMeshProUGUI reticleText, reticleNotifyText;


    private bool lookLoop = false;
    private Camera cam;
    private GameObject currentObj;
    private SelectedItemHandler selectedItemHandler;
    private PlayerActionManager playerActionManager;
    private PlayerInfoManager playerInfoManager;
    private bool showingError = false;

    private ItemData placeableItemData;
    private bool builderActive = false;
    private GameObject currentBuildObject;
    private List<GameObject> tempPlaceables = new List<GameObject>();
    public Material buildPreviewMaterial;
    public Material buildPreviewRedMaterial;
    private int activeItemSlot = 0;
    private CollideSensor collideSensor;
    private bool canPlace = false;


    private void Start()
    {
        //Check if Client and Initialize
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            playerActionManager = PlayerActionManager.singleton;
            playerInfoManager = PlayerInfoManager.singleton;
            lookLoop = true;
            regularLayerMask = LayerMask.GetMask("Clickable", "DeathDrop", "Resource");
            builderLayerMask = LayerMask.GetMask("Terrain");
            reticle = GetComponent<RectTransform>();
            pos = new Vector3(0.063f, 0.063f, 0.063F);
            pos2 = new Vector3(0.08f, 0.08f, 0.08f);
            cam = Camera.main;
            selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
        }
    }


    private void Update() 
    {
        if (CrossPlatformInputManager.GetButtonDown("Use"))
        {
            Use();
        }
    }

    //Raycast Loop Check
    private void FixedUpdate()
    {
        if(lookLoop && !builderActive) 
        {
            ShootRegularRaycast();
        }
        else if (lookLoop && builderActive) 
        {
            ShootBuilderRaycast();
        }
    }

    //Raycast: Shoot Regular
    private void ShootRegularRaycast() 
    {
        if (cam == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 15, regularLayerMask))
        {
            if (hit.collider != null)
            {
                currentObj = hit.collider.gameObject;
                reticle.localScale = pos2;
                if (!showingError)
                {
                    ShowTip();
                }
            }
        }
        else if (reticle.localScale != pos || reticleTip.activeSelf)
        {
            if (!showingError)
            {
                reticle.localScale = pos;
                reticleTip.SetActive(false);
                currentObj = null;
            }
        }
    }

    //Raycast: Shoot Builder
    private void ShootBuilderRaycast() 
    {
        if (cam == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 15, builderLayerMask))
        {
            if (hit.collider != null)
            {
                currentObj = hit.collider.gameObject;
                reticle.localScale = pos2;
                if (!showingError)
                {
                    UpdateBuildPreview(true, hit.point, hit.normal);
                }
            }
        }
        else if (reticle.localScale != pos || reticleTip.activeSelf)
        {
            if (!showingError)
            {
                UpdateBuildPreview(false, Vector3.zero, Vector3.zero);
                reticle.localScale = pos;
                reticleTip.SetActive(false);
                currentObj = null;
            }
        }
    }

    //Update Build Preview
    private void UpdateBuildPreview(bool activate, Vector3 position , Vector3 normal) 
    {
        if (activate) 
        {
            if(currentBuildObject == null)
            {
                if (placeableItemData != null && placeableItemData.placeableItem != null)
                {
                    bool hasTemp = false;
                    foreach (GameObject placeableObj in tempPlaceables)
                    {
                        if(placeableObj.name == placeableItemData.placeableItem.name + "(Clone)") 
                        {
                            hasTemp = true;
                            currentBuildObject = placeableObj;
                            currentBuildObject.SetActive(true);
                            collideSensor = currentBuildObject.GetComponent<CollideSensor>();
                            break;
                        }   
                    }
                    if (!hasTemp)
                    {
                        currentBuildObject = Instantiate(placeableItemData.placeableItem);
                        currentBuildObject.transform.up = normal;
                        currentBuildObject.transform.position = position;
                        Vector3 rot = currentBuildObject.transform.localRotation.eulerAngles;
                        currentBuildObject.transform.localRotation = Quaternion.Euler(rot.x, reticleTip.transform.rotation.eulerAngles.y, rot.z);
                        collideSensor = currentBuildObject.GetComponent<CollideSensor>();

                        BoxCollider boxCollider = currentBuildObject.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                        {
                            boxCollider.isTrigger = true;
                        }

                        if (collideSensor != null && collideSensor.isOverlapping) 
                        {
                            canPlace = false;
                            ChangePreviewMaterial(currentBuildObject, false);
                        }
                        else 
                        {
                            canPlace = true;
                            ChangePreviewMaterial(currentBuildObject, true);
                        }
                    }
                }
            }
            else 
            {
                currentBuildObject.transform.up = normal;
                currentBuildObject.transform.position = position;
                Vector3 rot = currentBuildObject.transform.localRotation.eulerAngles;
                currentBuildObject.transform.localRotation = Quaternion.Euler(rot.x, reticleTip.transform.rotation.eulerAngles.y, rot.z);
                if (collideSensor != null && collideSensor.isOverlapping)
                {
                    canPlace = false;
                    ChangePreviewMaterial(currentBuildObject, false);
                }
                else
                {
                    canPlace = true;
                    ChangePreviewMaterial(currentBuildObject, true);
                }
            }
        }
        else 
        {
            canPlace = false;
            currentBuildObject.SetActive(false);
            tempPlaceables.Add(currentBuildObject);
            currentBuildObject = null;
        }
    }

    public void ChangePreviewMaterial(GameObject buildPreview, bool green) 
    {
        if (green) 
        {
            buildPreview.GetComponent<PlaceableObject>().ChangeMaterial(buildPreviewMaterial);
        }
        else 
        {
            buildPreview.GetComponent<PlaceableObject>().ChangeMaterial(buildPreviewRedMaterial);
        }
    }

    //Activate Build Overlay
    public void ActivateBuilderOverlay(ItemData item, int itemSlot) 
    {
        activeItemSlot = itemSlot;
        placeableItemData = item;
        builderActive = true;
    }

    //Deactivate Build Overlay
    public void DeactivateBuilderOverlay() 
    {
        placeableItemData = null;
        builderActive = false;
        if(currentBuildObject != null)
        {
            UpdateBuildPreview(false, Vector3.zero, Vector3.zero);
        }
    }

    //Primary Use Function
    public void Use() 
    {
        if (placeableItemData != null) 
        {
            if (canPlace) 
            {
                playerActionManager.PlacePlaceable(currentBuildObject.transform, placeableItemData.itemID, activeItemSlot);
            }
        }
        else if(currentObj != null) 
        {
            if(playerActionManager != null) 
            {
                //Clickable clickable = currentObj.GetComponent<Clickable>();
                //if(clickable != null) 
                //{
                //    playerActionManager.InteractWithClickable(clickable.uniqueId);
                //    return;
                //}
                DeathDrop deathDrop = currentObj.GetComponent<DeathDrop>();
                if(deathDrop != null) 
                {
                    playerActionManager.InteractWithDeathDrop(deathDrop.unique, deathDrop.dropItems.ToArray());
                    return;
                }
                Resource resource = currentObj.GetComponent<Resource>();
                if (resource != null)
                {
                    playerActionManager.InteractWithResource(resource.uniqueId);
                    return;
                }

            }
        }
        else 
        {
            if(selectedItemHandler == null) 
            {
                selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
            }
            if (selectedItemHandler != null)
            {
                selectedItemHandler.Use();
            }
        }
    }

    //Show Reticle Tool Tip
    private void ShowTip()
    {
        //Clickable clickable = currentObj.GetComponent<Clickable>();
        //if (clickable != null)
        //{
        //    string toolTip = clickable.toolTip;
        //    if (toolTip.Length > 0)
        //    {
        //        reticleTip.SetActive(true);
        //        reticleText.text = toolTip;
        //    }
        //}
        DeathDrop deathDrop = currentObj.GetComponent<DeathDrop>();
        if (deathDrop != null)
        {
            string toolTip = deathDrop.toolTip;
            if (toolTip.Length > 0)
            {
                reticleTip.SetActive(true);
                reticleText.text = toolTip;
            }
        }
    }

    //Show Error Notift
    public void ShowErrorNotify(string notify) 
    {
        if (!showingError) 
        {
            showingError = true;
            reticleTip.SetActive(true);
            reticleText.text = notify;
            reticleText.color = new Color32(255, 74, 74, 255);
            StartCoroutine(ClearNotify());
        }
    }

    //Clear the Notification
    private IEnumerator ClearNotify() 
    {
        yield return new WaitForSeconds(1);
        showingError = false;
        reticleText.text = "";
        reticleText.color = new Color32(255, 255, 255, 255);
        reticleTip.SetActive(false);
    }

}