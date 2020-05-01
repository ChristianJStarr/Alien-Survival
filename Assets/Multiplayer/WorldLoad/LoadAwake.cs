using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;

public class LoadAwake : MonoBehaviour
{
    public GameObject loadScreen;
    public ControlControl controls;
    public TextMeshProUGUI loadScreenText, loadScreenText2;
    public float FadeRate = 0.2F;
    public float FadeRate2 = 3F;
    private Image image;
    private float targetAlpha, text1Target, text2Target;
    Animator animator;
    private bool textOff = false;
    void Start()
    {
        image = loadScreen.GetComponent<Image>();
        if (image == null)
        {
            
        }
        targetAlpha = 1.0f;
        text1Target = 1.0f;
        text2Target = 0.0f;
        controls.Hide();
    }
    void Update()
    {
        if(image != null) 
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
            }
        }
        else 
        {
            image = loadScreen.GetComponent<Image>();
        }
        
        if(loadScreenText != null) 
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
        
        if(loadScreenText2 != null) 
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
    public void ReadyWake() 
    {
        targetAlpha = 0.8f;
        text2Target = 1;
        text1Target = 1f;
        loadScreen.GetComponent<Button>().interactable = true;
    }

    public void WakeUp() 
    {
        FadeOut();
        if(animator == null) 
        {
            animator = FindObjectOfType<FirstPersonController>().GetComponent<Animator>();
        }
        animator.SetTrigger("Wake");
        animator.GetComponent<HeadLookController>().EnableHead();
        controls.Show();
    }
    public void FadeOut()
    {
        textOff = true;
        targetAlpha = 0.0f;
        text1Target = 0.0f;
        text2Target = 0.0f;
    }
}
