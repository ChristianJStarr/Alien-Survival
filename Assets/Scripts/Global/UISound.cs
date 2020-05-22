using UnityEngine;

public class UISound : MonoBehaviour
{
    private MusicManager manager;

    void Start()
    {
        manager = FindObjectOfType<MusicManager>();
    }

    public void Click() 
    {
        if(manager == null) 
        {
            manager = FindObjectOfType<MusicManager>();
            if (manager == null)
            {
                return;
            }
        }
        manager.ButtonClick();
    }
}
