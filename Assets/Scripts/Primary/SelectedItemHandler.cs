using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedItemHandler : MonoBehaviour
{
    public int curSlot = 0;
    public Item selectedItem;
    public ItemData selectedItemData;
    private InventoryGfx inventory;

    private GameObject holdItem;
    private List<GameObject> holdItemCache;

    public Animator animator;
    public Transform handAnchor;
    private GameObject rightHand;
    private GameObject leftHand;
    private bool ikActive = false;
    
    
    private void Start()
    {
        holdItemCache = new List<GameObject>();
        inventory = FindObjectOfType<InventoryGfx>();
        
    }

    public void Use() 
    {
        if(selectedItem == null || selectedItem.itemID == 0) 
        {
            //Hand is Empty, safe to punch.
        }
        else if(inventory != null && selectedItem.itemID == inventory.SelectSlot(curSlot).itemID)
        {
            Animator holdAnimator = holdItem.GetComponent<Animator>();
            if(holdAnimator != null) 
            {
                Debug.Log("fired weapon");
                holdAnimator.SetTrigger("Use");
            }
        }
    }

    public void SelectSlot(int slot)
    {
        if(inventory == null) 
        {
            inventory = FindObjectOfType<InventoryGfx>();
        }
        if (curSlot != slot) 
        {
            curSlot = slot;
            selectedItem = inventory.SelectSlot(slot);
            if(selectedItem != null) 
            {
                if (selectedItem.isHoldable)
                {
                    HoldItem();
                }
                else
                {
                    if (holdItem != null)
                    {
                        holdItem.SetActive(false);
                        holdItemCache.Add(holdItem);
                        leftHand = null;
                        rightHand = null;
                    }
                }
            }
            else 
            {
                if (holdItem != null)
                {
                    holdItem.SetActive(false);
                    holdItemCache.Add(holdItem);
                    leftHand = null;
                    rightHand = null;
                    Debug.Log("1");
                }
                else 
                {
                    Debug.Log("2");
                }
            }
        }
    }

    private void HoldItem() 
    {
        if (holdItem != null)
        {
            holdItem.SetActive(false);
            holdItemCache.Add(holdItem);
            leftHand = null;
            rightHand = null;
        }
        selectedItemData = inventory.FindItemData(selectedItem.itemID);
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
    }

    private void UpdateTargets()
    {
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget");
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget");
        ikActive = true;
    }
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
}
