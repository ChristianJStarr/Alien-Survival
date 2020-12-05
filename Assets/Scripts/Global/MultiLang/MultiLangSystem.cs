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

    //Change the Current System Lange
    public static void ChangeCurrentLanguage(int language) 
    {
        if (systemLanguage != language) 
        {
            systemLanguage = language;
            ChangedLanguage();
        }
    }

    //Get Language Data from String Key
    public static LangDataSingle GetLangDataFromKey(string key) 
    {
        LangDataSingle instance = null;
        if (langDictionary != null)
        {
            if (langDictionary.Count == 0)
            {
                PopulateLanguageDictionary();
            }
            if (langDictionary.ContainsKey(key))
            {
                LangData data = langDictionary[key];
                for (int i = 0; i < data.lang.Length; i++)
                {
                    if (data.lang[i].language == GetSystemLanguage())
                    {
                        return data.lang[i];
                    }
                }
            }
        }
        return instance;
    }

    //Populate the Language Dictionary
    private static void PopulateLanguageDictionary()
    {
        if (langDictionary.Count > 0)
        {
            langDictionary.Clear();
        }
        string path = Application.dataPath + "/ls-lang.txt";
        if (File.Exists(path))
        {
            LangData[] temp = JsonHelper.FromJson<LangData>(File.ReadAllText(path));
            for (int i = 0; i < temp.Length; i++)
            {
                langDictionary.Add(temp[i].key, temp[i]);
            }
        }
    }

    //Get The System Language
    private static int GetSystemLanguage() 
    {
        if(systemLanguage == 0)
        {
            SystemLanguage current = Application.systemLanguage;
            if (current == SystemLanguage.English) { systemLanguage = 10; }
            else if (current == SystemLanguage.Chinese) { systemLanguage = 6; }
            else if (current == SystemLanguage.German) { systemLanguage = 15; }
            else if (current == SystemLanguage.Russian) { systemLanguage = 30; }
            else if (current == SystemLanguage.French) { systemLanguage = 14; }
            else if (current == SystemLanguage.Japanese) { systemLanguage = 22; }
            else if (current == SystemLanguage.Korean) { systemLanguage = 23; }
        }
        return systemLanguage;
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
