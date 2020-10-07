using MLAPI;
using System.Collections;
using UnityEngine;

public class AudioDebugger : MonoBehaviour
{
    public string audioName;


    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer)
        {
            StartCoroutine(AudioLoop());
        }
        else 
        {
            Destroy(this);
        }
    }

    private IEnumerator AudioLoop()
    {
        while (true) 
        {
            yield return new WaitForSeconds(1);
            LocalSoundManager.Singleton.PlaySound(audioName, transform.position);
        }
    }
}
