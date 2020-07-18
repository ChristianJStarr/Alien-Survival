using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Jun_MultiLanguagePool : MonoBehaviour 
{
    [System.Serializable]
    public class LanguageData
    {
        [HideInInspector][SerializeField]string m_key;
        [HideInInspector] [SerializeField] Jun_MultiLanguage m_language;

        public string key{ get { return m_key; }}
        public Jun_MultiLanguage language{ get { return m_language; }}
    }

    [System.Serializable]
    public class LanguageTransportData 
    {
        public string key;
        public Jun_MultiLanguage lang;
        public string text;
        public int size;
    }


    [HideInInspector] [SerializeField] List<LanguageData> m_languageDatas = new List<LanguageData>();


    private void Start()
    {
        //LoadStoredLanguage();
        StoreStoredLanguage();
    }

    private void LoadStoredLanguage() 
    {
        string json = File.ReadAllText(Application.dataPath + "/Content/ExtData/ls-lang.txt");
        LanguageTransportData[] langData = JsonHelper.FromJson<LanguageTransportData>(json);
        if(langData.Length > 0) 
        {
            foreach (LanguageTransportData data in langData)
            {
                Debug.Log(data.key + " : " + data.text);
            }
        }
    }

    private void StoreStoredLanguage() 
    {
        List<LanguageTransportData> langData = new List<LanguageTransportData>();
        foreach  (LanguageData data in m_languageDatas)
        {
            LanguageTransportData newData = new LanguageTransportData();
            newData.key = data.key;
            newData.lang = data.language;
            langData.Add(newData);
        }
        string json = JsonHelper.ToJson(langData.ToArray());
        Debug.Log(json);
    }


    public string[] keys
    {
        get
        {
            string[] keyNames = new string[m_languageDatas.Count];
            for (int i = 0; i < m_languageDatas.Count; i++)
            {
                keyNames[i] = m_languageDatas[i].key;
            }
            return keyNames;
        }
    }

    public Jun_MultiLanguage GetLanguage (int index)
    {
        if (index < m_languageDatas.Count)
            return m_languageDatas[index].language;
        return null;
    }

    public Jun_MultiLanguage GetLanguage (string key)
    {
        for (int i = 0; i < m_languageDatas.Count; i++)
        {
            if (key == m_languageDatas[i].key)
                return m_languageDatas[i].language;
        }
        return null;
    }

    public int GetKeyID (string key)
    {
        for (int i = 0; i < m_languageDatas.Count; i++)
        {
            if (key == m_languageDatas[i].key)
                return i;
        }
        return -1;
    }

    public string GetKey (int index)
    {
        if (index < m_languageDatas.Count)
            return m_languageDatas[index].key;
        return "";
    }
}
