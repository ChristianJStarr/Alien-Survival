using UnityEngine;

public class SelectedItemHandler : MonoBehaviour
{
    public Item selectedItem;
    public Animator animator;
    public HoldableManager holdableManager;
    public ItemData selectedItemData;
    private InventoryGfx inventory;
    private Transform rightHand;
    private Transform leftHand;
    private ControlControl controls;
    private Reticle reticle;
    private bool ikActive = false;
    private int selectedSlot = 1;


    private void Start()
    {
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

    //Selected Item Return Response
    public void SelectedReturn(bool success) 
    {
        if (success) 
        {
            holdableManager.UseHoldable(selectedItemData.holdableId);
        }
        else 
        {
            if(selectedItemData != null) 
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
        reticle.activeItemSlot = slot;
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
                    controls.UpdateUseIcon(0);
                }
                ClearHoldItem();
                DeactivateBuilderOverlay();
            }
        }
    }

    //Hold Item Function
    private void ShowHoldableItem() 
    {
        selectedItemData = InvUI.FindItemDataById(selectedItem.itemID); //Get ItemData from ItemID

        SetControlUseType(selectedItemData.useType); //Set Control Use Type Icon
        holdableManager.PulloutHoldable(selectedItemData.holdableId);
        UpdateTargets();
    }

    //Show Placeable Item
    private void ShowPlaceableItem()
    {
        selectedItemData = InvUI.FindItemDataById(selectedItem.itemID); //Get ItemData from ItemID
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
        GameObject left = GameObject.FindGameObjectWithTag("LHandTarget");
        if(left != null) 
        {
            leftHand = left.transform;
        }
        GameObject right = GameObject.FindGameObjectWithTag("RHandTarget");
        if (right != null) 
        {
            rightHand = right.transform;
        }
        ikActive = true;
    }

    //On Animator Ik
    void OnAnimatorIK(int index)
    {
        if (index == 2 && animator) 
        {
            if (ikActive)
            {
                if (rightHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.rotation);
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
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.rotation);
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

    //Clear HoldItem
    private void ClearHoldItem() 
    {
        holdableManager.PulloutHoldable(0);
        ikActive = false;
    }

    //Set Control Use Type
    private void SetControlUseType(int useType) 
    {
        if (controls != null)
        {
            controls.UpdateUseIcon(useType);
        }
    }

}
