using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private void OnValidate()
    {
        UpdateText();
    }



    private void UpdateText() 
    {
        if (key.Length > 0)
        {
            LangDataSingle langDatas = MultiLangSystem.GetLangDataFromKey(key);
            if (langDatas != null)
            {
                if (text == null)
                {
                    text = GetComponent<TextMeshProUGUI>();
                }
                if (text != null)
                {
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
            }
        }
    }
}
