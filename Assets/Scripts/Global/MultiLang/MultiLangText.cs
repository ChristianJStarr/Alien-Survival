using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class MultiLangText : MonoBehaviour
{
    public string key = "";
    private TextMeshProUGUI text;
    private float storedFontSize = 0;


    private void OnEnable()
    {
        MultiLangSystem.ChangedLanguage += UpdateText;
    }
    private void OnDisable()
    {
        MultiLangSystem.ChangedLanguage -= UpdateText;
    }


    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (key.Length > 0)
        {
            LangDataSingle langDatas = MultiLangSystem.GetLangDataFromKey(key);
            if (langDatas != null)
            {
                if (!text)
                {
                    text = GetComponent<TextMeshProUGUI>();
                }
                if (storedFontSize == 0)
                {
                    storedFontSize = text.fontSize;
                }
                text.text = langDatas.text;
                if (langDatas.fontSize == 0)
                {
                    if (text.fontSize != storedFontSize)
                    {
                        text.fontSize = storedFontSize;
                    }
                }
                else
                {
                    text.fontSize = langDatas.fontSize;
                }
            }
            else { Debug.Log("NULL"); }
        }
    }
}
