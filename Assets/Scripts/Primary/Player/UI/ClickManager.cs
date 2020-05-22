using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ClickManager : MonoBehaviour
{
    public Inventory inventory;

    void FixedUpdate()
    {
        if (CrossPlatformInputManager.GetButtonDown("Use"))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 15, LayerMask.GetMask("Clickable")))
            {
                if (hit.collider != null)
                {
                    Debug.Log("Click Manager - Clicked " + hit.collider.gameObject.name);

                    //if (hit.collider.gameObject.GetComponent<DroppedItem>() != null)
                    //{
                    //    Debug.Log("ClickMgr - Clicked Dropped Item");
                    //    DroppedItem item = hit.collider.gameObject.GetComponent<DroppedItem>();
                    //    if (inventory.AddNewItem(item.itemId, 44, 0, item.itemStack, item.itemSpecial))
                    //    {
                    //        Destroy(hit.collider.gameObject);
                    //    }
                    //}
                    //else
                    //{
                    //    Debug.Log("use item in hand");
                    //}
                }
            }
        }
    }
}