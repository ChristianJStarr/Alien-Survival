using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MultiLangSystem
{
    public static Dictionary<string, LangData> langDictionary = new Dictionary<string, LangData>();
    private static int systemLanguage = 10;

    public delegate void OnLanguageChangeDelegate();
    public static event OnLanguageChangeDelegate ChangedLanguage;

    public static void ChangeCurrentLanguage(int language) 
    {
        if (systemLanguage != language) 
        {
            systemLanguage = language;
            ChangedLanguage();
        }
    }


    public static LangDataSingle GetLangDataFromKey(string key) 
    {
        if(langDictionary != null && langDictionary.Count == 0) 
        {
            GetLanguageDatas();
        }
        if (langDictionary.Count > 0)
        {
            return FindLanguageData(key);
        }
        else 
        {
            return null;
        }
    }

    private static LangDataSingle FindLanguageData(string key) 
    {
        if (langDictionary != null && langDictionary.ContainsKey(key)) 
        {
            LangData data = langDictionary[key];
            if (data != null)
            {
                foreach (LangDataSingle single in data.lang)
                {
                    if (single.language == GetSystemLanguage())
                    {
                        return single;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        else 
        {
            return null;
        }
    }

    private static void GetLanguageDatas()
    {
        LangData data = new LangData();
        TextAsset langDataText = Resources.Load("ls-lang") as TextAsset;
        LangData[] tempDatas = JsonHelper.FromJson<LangData>(langDataText.text);
        if (langDictionary.Count > 0)
        {
            langDictionary.Clear();
        }
        foreach (LangData temp in tempDatas)
        {
            langDictionary.Add(temp.key, temp);
        }
    }

    private static int GetSystemLanguage() 
    {
        if(systemLanguage != 0) 
        {
            return systemLanguage;
        }
        else 
        {
            SystemLanguage current = Application.systemLanguage;
            if (current == SystemLanguage.English) { return 10; }
            else if (current == SystemLanguage.Chinese) { return 6; }
            else if (current == SystemLanguage.German) { return 15; }
            else if (current == SystemLanguage.Russian) { return 30; }
            else if (current == SystemLanguage.French) { return 14; }
            else if (current == SystemLanguage.Japanese) { return 22; }
            else if (current == SystemLanguage.Korean) { return 23; }
            return 10;
        }
    }
}

[Serializable]
public class LangData 
{
    public string key;
    public LangDataSingle[] lang;
}
[Serializable]
public class LangDataSingle 
{
    public int language;
    public string text;
    public float fontSize;
}
