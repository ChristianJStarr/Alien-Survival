using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            ScreenCapture.CaptureScreenshot("background.png", 4);
        }
    }
}
