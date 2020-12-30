using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugTextChange : MonoBehaviour
{
    private TextMeshProUGUI text;
    private int inc = 0;
    
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }



    void Update() 
    {
        if (!text) return;
        if (inc > 10000)
        {
            inc = 0;
        }
        inc++;
        text.text = inc.ToString();
    }
}
