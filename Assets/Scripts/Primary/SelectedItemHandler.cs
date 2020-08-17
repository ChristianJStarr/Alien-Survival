using System.Collections.Generic;
using UnityEngine;

public class SelectedItemHandler : MonoBehaviour
{
    public Item selectedItem;
    public ItemData selectedItemData;
    private InventoryGfx inventory;

    private GameObject holdItem;
    private List<GameObject> holdItemCache;

    public Transform aimTransform;
    public Animator animator;
    public Transform handAnchor;
    private GameObject rightHand;
    private GameObject leftHand;
    private ControlControl controls;
    private GameServer gameServer;
    private PlayerActionManager actionManager;
    private PlayerInfoManager infoManager;
    private Reticle reticle;

    private bool ikActive = false;
    private int selectedSlot = 1;
    private bool isHoldingUse = false;

    private Animator tempAnimator;

    private void Start()
    {
        actionManager = PlayerActionManager.singleton;
        infoManager = PlayerInfoManager.singleton;
        gameServer = GameServer.singleton;
        holdItemCache = new List<GameObject>();
        inventory = FindObjectOfType<InventoryGfx>();
        reticle = FindObjectOfType<Reticle>();
        controls = inventory.GetComponent<ControlControl>();
        inventory.SelectedItemHandover(this);
    }


    //Update the Selected Slot
    public void UpdateSelectedSlot() 
    {
        SelectSlot(selectedSlot);
    }

    //Use Selected Item
    public void Use() 
    {
        actionManager.UseSelectedItem(selectedSlot, aimTransform);
    }

    //Selected Item Return Response
    public void SelectedReturn(bool success) 
    {
        if (success) 
        {
            if (tempAnimator != null)
            {
                tempAnimator.SetTrigger("Use");
            }
        }
        else 
        {
            int type = selectedItemData.useType;
            if (type == 1)
            {
                ShowReticleNotify("OUT OF AMMO");
            }
            else if (type == 3)
            {
                ShowReticleNotify("NOT ENOUGH ROOM");
            }
        }
    }

    //Show Reticle ToolTip

    private void ShowReticleNotify(string notify) 
    {
        if(reticle == null) 
        {
            reticle = FindObjectOfType<Reticle>();
        }
        reticle.ShowErrorNotify(notify);
    }

    //Select a Slot (int)
    public void SelectSlot(int slot)
    {
        if(inventory == null) 
        {
            inventory = FindObjectOfType<InventoryGfx>();
        }
        if(inventory != null) 
        {
            selectedItem = inventory.SelectSlot(slot);
            if(selectedItem != null) 
            {
                if (selectedItem.isPlaceable) 
                {
                    ShowPlaceableItem();
                }
                else if (selectedItem.isHoldable) 
                {
                    DeactivateBuilderOverlay();
                    ShowHoldableItem();
                }
                else {
                    DeactivateBuilderOverlay();
                    ClearHoldItem();
                }
            }
            else
            {
                if (controls != null)
                {
                    controls.SwapUse(0);
                }
                ClearHoldItem();
                DeactivateBuilderOverlay();
            }
        }
    }

    //Hold Item Function
    private void ShowHoldableItem() 
    {
        selectedItemData = inventory.FindItemData(selectedItem.itemID); //Get ItemData from ItemID
        ClearHoldItem(); //Clear Hold Item
        SetControlUseType(selectedItemData.useType); //Set Control Use Type Icon
        bool selected = false;
        foreach (GameObject obj in holdItemCache)
        {
            if (obj.name == selectedItemData.holdableObject.name + "(Clone)")
            {
                obj.SetActive(true);
                selected = true;
                UpdateTargets();
                holdItem = obj;
                break;
            }
        }
        if (handAnchor != null && selected == false)
        {
            holdItem = Instantiate(selectedItemData.holdableObject, handAnchor);
            UpdateTargets();
        }
        tempAnimator = holdItem.GetComponent<Animator>();
    }

    //Show Placeable Item
    private void ShowPlaceableItem()
    {
        selectedItemData = inventory.FindItemData(selectedItem.itemID); //Get ItemData from ItemID
        ClearHoldItem(); //Clear Hold Item
        SetControlUseType(selectedItemData.useType); //Set Control Use Type Icon

        //Get Prefab from ItemData and Spawn it infront of player when activated
        ActivateBuilderOverlay();
    }

    //Activate Builder
    private void ActivateBuilderOverlay() 
    {
        reticle.ActivateBuilderOverlay(selectedItemData, selectedSlot);
    }
    
    //Deactivate Builder
    private void DeactivateBuilderOverlay() 
    {
        reticle.DeactivateBuilderOverlay();
    }

    //Update Animator Targets
    private void UpdateTargets()
    {
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget");
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget");
        ikActive = true;
    }
    
    //On Animator Ik
    void OnAnimatorIK(int index)
    {
        if (index == 2) 
        {
            if (animator)
            {
                if (ikActive)
                {

                    if (rightHand != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.transform.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.transform.rotation);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    }
                    if (leftHand != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.transform.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.transform.rotation);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                    }
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
        }
        
    }

    //Clear HoldItem
    private void ClearHoldItem() 
    {
        if (holdItem != null)
        {
            holdItem.SetActive(false);
            holdItemCache.Add(holdItem);
            leftHand = null;
            rightHand = null;
        }
    }

    //Set Control Use Type
    private void SetControlUseType(int useType) 
    {
        if (controls != null)
        {
            controls.SwapUse(useType);
        }
    }

}
