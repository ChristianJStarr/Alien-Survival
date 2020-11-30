using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;
using UnityEngine.Rendering;

public class LoadAwake : MonoBehaviour
{
    public static LoadAwake Singleton;

    public TimeSystem timeSystem;
    public GameObject loadScreen, topBar, inventory;
    public Volume volume;
    public TextMeshProUGUI loadScreenText, loadScreenText2;
    public float FadeRate = 0.1F;
    public float FadeRate2 = 2F;
    private Image image;
    private float targetAlpha, text1Target, text2Target;
    private Animator animator;
    private bool textOff = false;
    private ControlControl controls;
    private bool readyToWake = false;
    private bool sleeping = false;

    public bool playerHasInfo = false;
    public bool playerHasCamera = false;


    void Awake() 
    {
        Singleton = this;
    }
    void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            EnterSleepState();
        }
        else 
        {
            Destroy(this);
        }
    }
    void Update()
    {
        if (sleeping && loadScreen.activeSelf) 
        {
            ColorFade();
        } 
        if(!readyToWake && playerHasInfo && playerHasCamera) 
        {
            timeSystem.ForceCheckTime();
            ReadyWake();
        }
    }
    
    //Enter the Sleep State
    public void EnterSleepState() 
    {
        if (!sleeping)
        {
            if (controls == null)
            {
                controls = GetComponent<ControlControl>();
            }
            
            if (image == null)
            {
                image = loadScreen.GetComponent<Image>();
            }
            loadScreen.SetActive(true);
            InterfaceHider.Singleton.HideAllInterfaces();
            targetAlpha = 1.0f;
            text1Target = 1.0f;
            text2Target = 0.0f;
            Cursor.visible = true;
            sleeping = true;
            if (CheckPlayerAnimator())
            {
                animator.SetTrigger("Sleep");
            }
        }
    }

    //Ready to Wakeup Function
    private void ReadyWake() 
    {
        if (!readyToWake)
        {
            readyToWake = true;
            targetAlpha = .3f;
            text2Target = .73f;
            text1Target = .73f;
            loadScreen.GetComponent<Button>().interactable = true;
        }
    }

    //Wake up Function
    public void WakeUp() 
    {
        if (readyToWake)
        {
            FadeOut();
            if (CheckPlayerAnimator())
            {
                animator.SetTrigger("Wake");
            }
        }
    }

    //Fade out overlay
    private void FadeOut()
    {
        textOff = true;
        targetAlpha = 0.0f;
        text1Target = 0.0f;
        text2Target = 0.0f;
    }
    
    //Color fade updater
    private void ColorFade() 
    {
        if(targetAlpha == 0.0f) 
        {
            FadeRate = 3F;
            FadeRate2 = 3F;
        }

        if (image != null)
        {
            Color curColor = image.color;
            float alphaDiff = Mathf.Abs(curColor.a - targetAlpha);
            if (alphaDiff > 0.0001f)
            {
                curColor.a = Mathf.Lerp(curColor.a, targetAlpha, FadeRate * Time.deltaTime);
                image.color = curColor;
                
            }
            if (alphaDiff < 0.1f && targetAlpha == 0.0f)
            {
                loadScreen.SetActive(false);
                volume.weight = 0;
                InterfaceHider.Singleton.ShowAllInterfaces();
                sleeping = false;
            }
        }
        else
        {
            image = loadScreen.GetComponent<Image>();
        }

        if (loadScreenText != null)
        {
            Color text1Color = loadScreenText.color;
            float text1Diff = Mathf.Abs(text1Color.a - text1Target);
            if (text1Diff > 0.0001f)
            {
                text1Color.a = Mathf.Lerp(text1Color.a, text1Target, FadeRate2 * Time.deltaTime);
                loadScreenText.color = text1Color;
            }
            if (text1Diff < 0.1f && textOff)
            {
                loadScreenText.enabled = false;
            }
        }

        if (loadScreenText2 != null)
        {
            Color text2Color = loadScreenText2.color;
            float text2Diff = Mathf.Abs(text2Color.a - text2Target);
            if (text2Diff > 0.0001f)
            {
                text2Color.a = Mathf.Lerp(text2Color.a, text2Target, FadeRate2 * Time.deltaTime);
                if (loadScreenText.enabled == false)
                {
                    loadScreenText.enabled = true;
                }
                loadScreenText2.color = text2Color;
            }
            if (text2Diff < 0.1f && textOff)
            {
                loadScreenText2.enabled = false;
            }
        }
    }

    //Check if Animator Exists / Grabs Animator from PlayerObject
    private bool CheckPlayerAnimator() 
    {
        if (animator == null) 
        {
            if (NetworkingManager.Singleton != null)
            {
                NetworkedObject playerObject = NetworkingManager.Singleton.ConnectedClients[NetworkingManager.Singleton.LocalClientId].PlayerObject;
                if (playerObject != null)
                {
                    animator = playerObject.GetComponent<Animator>();
                    return true;
                }
                return false;
            }
            return false;
        }
        return true;
    }
}
