using UnityEngine;

public class UI_Sound : MonoBehaviour
{
#if !UNITY_SERVER
    public int ui_soundId = 0;

    public void Click() 
    {
        MusicManager.PlayUISound(ui_soundId);
    }
#endif
}
