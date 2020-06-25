using UnityEngine;

public class SelectedItemButtons : MonoBehaviour
{
    SelectedItemHandler selectedItemHandler;

    void Start()
    {
        selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
    }

    //Select Slot
    public void SelectSlot(int id)
    {
        if(selectedItemHandler == null) 
        {
            selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
        }
        selectedItemHandler.SelectSlot(id);
    }
}
