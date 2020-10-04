using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaUIWidthBalanced : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        RefreshPanel(Screen.safeArea);
    }

//#if UNITY_EDITOR
//    private void OnValidate() 
//    {
//        _rectTransform = GetComponent<RectTransform>();
//        RefreshPanel(Screen.safeArea);
//    }
//#endif


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
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y = _rectTransform.anchorMin.y;
        anchorMax.x /= Screen.width;
        anchorMax.y = _rectTransform.anchorMax.y;

        if(anchorMin.x == 0 && anchorMax.x != 1) 
        {
            anchorMin.x = 1 - anchorMax.x;
        }
        else if (anchorMax.x == 1 && anchorMin.x != 0)
        {
            anchorMax.x = 1 - anchorMin.x;
        }

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
    }
}
