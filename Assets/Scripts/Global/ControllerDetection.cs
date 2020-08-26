using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControllerDetection : MonoBehaviour
{
    public TextMeshProUGUI notifyText;
    public RectTransform notifyObject;
    
    
    private bool connected = false;
    private Vector2 notifyTarget = new Vector3 (0,120);
    private bool showNotify = false;


    void Awake()
    {
        StartCoroutine(CheckForControllers());
        DontDestroyOnLoad(gameObject);
    }

    private void Update() 
    {
        if(notifyObject.anchoredPosition != notifyTarget) 
        {
            notifyObject.anchoredPosition = Vector2.MoveTowards(notifyObject.anchoredPosition, notifyTarget, 200F * Time.deltaTime);
            StartCoroutine(CloseNotify());
        }
    }

    IEnumerator CheckForControllers()
    {
        while (true)
        {
            var controllers = Input.GetJoystickNames();
            if (!connected && controllers.Length > 0)
            {
                connected = true;
                ControllerNotify();
            }
            else if (connected && controllers.Length == 0)
            {
                connected = false;
                ControllerNotify();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator CloseNotify() 
    {
        yield return new WaitForSeconds(5F);
        showNotify = false;
        notifyTarget = new Vector2(0, 120);
    }

    private void ControllerNotify() 
    {
        if (connected) 
        {
            notifyText.text = "Controller Connected";
        }
        else 
        {
            notifyText.text = "Controller Disconnected";
        }
        showNotify = true;
        notifyTarget = new Vector2(0, -120);
    }
}
