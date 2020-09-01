using MLAPI;
using System.Collections;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager singleton;

    public TextMeshProUGUI messageBox;
    private int readtime = 0;

    private void Awake() 
    {
        singleton = this;
    }

    private void Start() 
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            StartCoroutine(SetChatVisible());
        }
        else 
        {
            Destroy(this);
        }
    }

    public void Incoming(string message) 
    {
        messageBox.text += "<br>" + message;
        readtime += message.Split(' ').Length;
    }

    private IEnumerator SetChatVisible() 
    {
        WaitForSeconds wait = new WaitForSeconds(1F);
        while (true) 
        {
            yield return wait;
            if (readtime > 0)
            {
                if (!messageBox.enabled)
                {
                    messageBox.enabled = true;
                }
                readtime--;
            }
            else if (messageBox.enabled)
            {
                messageBox.enabled = false;
            }
        }
    }

}
