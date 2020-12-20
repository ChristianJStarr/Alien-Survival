using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingScreenText : MonoBehaviour
{
    public TextMeshProUGUI loading_text;
    public float tip_time = 5;
    public string[] loadingTipKeys = new string[]
    {
        "tip_1",
        "tip_2",
        "tip_3",
        "tip_4",
        "tip_5",
        "tip_6"
    };
    

    private void Start() 
    {
        int length = loadingTipKeys.Length;
        int startIndex = UnityEngine.Random.Range(0, length);
        if (length > 0)
        {
            StartCoroutine(LoadingTipLoop(startIndex, length));
        }
    }

    private IEnumerator LoadingTipLoop(int startIndex, int length) 
    {
        int empty_detect = 0;
        while (enabled) 
        {
            if(empty_detect == length) 
            {
                break;
            }
            if(startIndex == length) 
            {
                startIndex = 0;
            }
            LangDataSingle data = MultiLangSystem.GetLangDataFromKey(loadingTipKeys[startIndex]);
            if (data != null)
            {
                if (data.fontSize != 0) loading_text.fontSize = data.fontSize;
                loading_text.text = data.text;
                yield return new WaitForSeconds(data.text.Split(' ').Length);
            }
            else 
            {
                empty_detect++;
            }
            startIndex++;
        }
    }
}
