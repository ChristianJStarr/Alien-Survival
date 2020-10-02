using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Singleton;
    private InterfaceMenu[] interfaceMenus;

    void Awake() 
    {
        Singleton = this;
    }

    private void Start()
    {
        interfaceMenus = FindObjectsOfType<InterfaceMenu>();
    }

    public void EnableMenu(int menuId, string data) 
    {
        InterfaceMenu menu = GetMenuById(menuId);
        if(menu != null) 
        {
            menu.Enable(data);
        }
        else 
        {
            DebugMsg.Notify("Interface Menu Not Found!", 2);
        }
    }


    public void UpdateMenu(int menuId, string data)
    {
        InterfaceMenu menu = GetMenuById(menuId);
        if (menu != null)
        {
            menu.UpdateData(data);
        }
        else
        {
            DebugMsg.Notify("Interface Menu Not Found!", 2);
        }
    }


    public void DisableMenu(int menuId)
    {
        InterfaceMenu menu = GetMenuById(menuId);
        if (menu != null)
        {
            menu.Disable();
        }
        else
        {
            DebugMsg.Notify("Interface Menu Not Found!", 2);
        }
    }

    private InterfaceMenu GetMenuById(int menuId) 
    {
        foreach (InterfaceMenu item in interfaceMenus)
        {
            if(item.interfaceId == menuId) 
            {
                return item;
            }
        }
        return null;
    }

}


