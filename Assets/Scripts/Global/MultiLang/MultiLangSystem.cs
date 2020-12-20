using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MultiLangSystem : MonoBehaviour
{
    #region Singleton 
    public static MultiLangSystem Singleton;
    private void Awake() 
    {
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    #region DelegateEvent
    public delegate void OnLanguageChangeDelegate();
    public static event OnLanguageChangeDelegate ChangedLanguage;
    #endregion
    
    public string language_DataPath = "/Data/ls-lang.txt";
    private Dictionary<string, LangData> dictionary = new Dictionary<string, LangData>();
    public int systemLanguage = 6;


    private void Start() 
    {
        systemLanguage = GetSystemLanguage();
        PopulateLanguageDictionary();
    }


    //Populate the Language Dictionary
    private void PopulateLanguageDictionary()
    {
        string path = Application.dataPath + language_DataPath;
        if (File.Exists(path))
        {
            DebugMsg.Notify("Populating Language Dictionary.", 1);
            LangData[] temp = JsonHelper.FromJson<LangData>(File.ReadAllText(path));
            for (int i = 0; i < temp.Length; i++)
            {
                dictionary.Add(temp[i].key, temp[i]);
            }
        }
    }


    //Change the Current System Lange
    public static void ChangeCurrentLanguage(int language)
    {
        if(Singleton != null) 
        {
            Singleton.ChangeCurrentLanguage_Task(language);
        }
    }
    private void ChangeCurrentLanguage_Task(int language) 
    {
        if (systemLanguage != language)
        {
            systemLanguage = language;
            ChangedLanguage?.Invoke();
        }
    }


    //Get The System Language
    private int GetSystemLanguage()
    {
        if (systemLanguage == 0)
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


    //Get Language Data From Key
    public static LangDataSingle GetLangDataFromKey(string key) 
    {
        if(Singleton != null) 
        {
            return Singleton.GetLangDataFromKey_Task(key);
        }
        return null;
    }
    private LangDataSingle GetLangDataFromKey_Task(string key) 
    {
        DebugMsg.Notify("Getting LangData. " + key, 1);
        int sysLanguage = GetSystemLanguage();
        if (dictionary.ContainsKey(key))
        {
            LangData data = dictionary[key];
            for (int i = 0; i < data.lang.Length; i++)
            {
                if (data.lang[i].language == sysLanguage)
                {
                    return data.lang[i];
                }
            }
        }
        return null;
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
