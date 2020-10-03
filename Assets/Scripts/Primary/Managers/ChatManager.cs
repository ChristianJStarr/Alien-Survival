using MLAPI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{


    public enum State : byte
    {
        Hidden = 1,
        AlwaysOpen = 2,
        OpenWhen = 3
    }
    public static ChatManager singleton;
    public TextMeshProUGUI messageBox;
    public GameObject boxContainer;
    public GameObject buttonBkg;
    public GameObject chatBoxContainer;
    public Settings settings;
    private int readtime = 0;
    private State state;
    
    [SerializeField] private Sprite btnState_Hidden, btnState_AlwaysOpen, btnState_OpenWhen;
    [SerializeField] private Image btnImage;


    private void Awake() 
    {
        singleton = this;
    }

    private void Start() 
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            state = (State)settings.chatBoxState;
            UpdateBoxState();
        }
        else 
        {
            Destroy(this);
        }
    }

    //Handle the Incoming Message from the Server
    public void Incoming(string message) 
    {
        messageBox.text += "<br>" + message;
        if(state == State.OpenWhen) 
        {
            readtime += message.Split(' ').Length;
            boxContainer.SetActive(true);
            StartCoroutine(HideChatWhenFinished());
        }
    }

    //Hide the Chat Box Container when Finished.
    private IEnumerator HideChatWhenFinished() 
    {
        WaitForSeconds wait = new WaitForSeconds(1F);
        while (true) 
        {
            yield return wait;
            if(state == State.OpenWhen) 
            {
                if (readtime > 0)
                {
                    readtime--;
                }
                else if (boxContainer.activeSelf)
                {
                    boxContainer.SetActive(false);
                }
            }
            else 
            {
                readtime = 0;
            }
        }
    }

    //Function for the Chat Toggle Button
    public void ChangeVisible() 
    {
        Debug.Log(state.ToString());
        Debug.Log("Changing Visible");
        if(state == State.OpenWhen) 
        {
            state = State.Hidden;
        }
        else if(state == State.Hidden) 
        {
            state = State.AlwaysOpen;
        }
        else if(state == State.AlwaysOpen) 
        {
            state = State.OpenWhen;
        }
        settings.chatBoxState = (byte)state;
        UpdateBoxState();
    }

    //Update Box Container State
    private void UpdateBoxState() 
    {
        if (state == State.OpenWhen)
        {
            btnImage.sprite = btnState_OpenWhen;
            boxContainer.SetActive(false);
            buttonBkg.SetActive(true);
        }
        else if (state == State.Hidden)
        {
            btnImage.sprite = btnState_Hidden;
            boxContainer.SetActive(false);
            buttonBkg.SetActive(true);
        }
        else if (state == State.AlwaysOpen)
        {
            btnImage.sprite = btnState_AlwaysOpen;
            boxContainer.SetActive(true);
            buttonBkg.SetActive(false);
        }
        Debug.Log("Finished Updating Box State");
    }


    public void Hide() 
    {
        chatBoxContainer.SetActive(false);
    }
    public void Show() 
    {
        chatBoxContainer.SetActive(true);
    }

}
