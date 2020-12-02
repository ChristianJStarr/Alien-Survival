using UnityEngine;

public class UI_HotbarButtons : MonoBehaviour
{
    public HoldableManager holdableManager;
    public PlayerCommandManager commandManager;
    public UI_Inventory inventory;
    private int selectedSlot;
    

    //Select Slot
    public void SelectSlot(int id)
    {
        if(selectedSlot != id) 
        {
            selectedSlot = id;
        }
        else 
        {
            selectedSlot = 0;
        }
        inventory.SetSlotFocused(selectedSlot); // Set this slot to focused
        commandManager.selectedSlot = selectedSlot;
    }
}
