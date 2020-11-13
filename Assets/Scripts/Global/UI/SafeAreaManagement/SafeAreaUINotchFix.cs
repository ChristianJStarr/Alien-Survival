using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaUINotchFix : MonoBehaviour
{

    #if !UNITY_SERVER
    private RectTransform _rectTransform;

    private bool flattenSide = false;



    private void Start()
    {
        string[] errorNotchDevices = { "iPhone10,3", "iPhone10,6", "iPhone11,8", "iPhone11,2", "iPhone11,6", "iPhone11,4", "iPhone12,1", "iPhone12,3", "iPhone12,5" };











        _rectTransform = GetComponent<RectTransform>();
        //Find devices with fucked up notch safe areas.

        if (UnityEngine.iOS.Device.generation.ToString() != "Unknown") 
        {
            if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone11ProMax
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone11Pro
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone11
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXR
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXS
                || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXSMax)
            {
                flattenSide = true;
                DebugMsg.Notify("Notched Iphone Detected. Fixing Inventory UI", 3);
            }
        }
        else
        {
            string device = SystemInfo.deviceModel;
            foreach (string item in errorNotchDevices)
            {
                if(item == device) 
                {
                    flattenSide = true;
                }
            }
        }

        
        Debug.Log("DEVICE:" + UnityEngine.iOS.Device.generation.ToString());
        RefreshPanel(Screen.safeArea);
    }

    private void OnEnable()
    {
        SafeAreaDetection.OnSafeAreaChanged += RefreshPanel;
    }

    private void OnDisable()
    {
        SafeAreaDetection.OnSafeAreaChanged -= RefreshPanel;
    }

    private void RefreshPanel(Rect safeArea)
    {
        if (flattenSide) 
        {
            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                //Notch is on left, flatten right
                Vector2 anchorMin = safeArea.position;
                Vector2 anchorMax = safeArea.position + safeArea.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x = 1;
                anchorMax.y /= Screen.height;

                _rectTransform.anchorMin = anchorMin;
                _rectTransform.anchorMax = anchorMax;

            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                //Notch is on right, flatten left
                Vector2 anchorMin = safeArea.position;
                Vector2 anchorMax = safeArea.position + safeArea.size;

                anchorMin.x = 0; 
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                _rectTransform.anchorMin = anchorMin;
                _rectTransform.anchorMax = anchorMax;

            }
        }
        else 
        {
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
#endif
}
