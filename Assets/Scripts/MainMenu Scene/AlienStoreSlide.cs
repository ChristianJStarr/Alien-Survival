
using UnityEngine;
using UnityEngine.UI;

public class AlienStoreSlide : MonoBehaviour
{
    public GameObject lockCover, pointObject, purchased, popup;
    public int itemId = 0;
    public int exp = 0;
    private MainMenuAlienStore alienStore;
    public Button mainButton;
    public void SetSlide(bool isLocked, bool isPurchased) 
    {
        lockCover.SetActive(isLocked);
        mainButton.interactable = !isLocked;
        purchased.SetActive(isPurchased);
        pointObject.SetActive(!isPurchased);
    }

    public void PurchasePop() 
    {
        popup.SetActive(true);
    }
    public void PurchaseItem() 
    {
        if(alienStore == null) 
        {
            alienStore = FindObjectOfType<MainMenuAlienStore>();
        }
        alienStore.PurchaseItem(itemId);
        popup.SetActive(false);
    }

}
