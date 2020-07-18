using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Jun_Language
{
	public SystemLanguage systemLanguage = SystemLanguage.English;
	public string languageText = "";
    public int fontSize = 60;

    public Jun_Language ()
    {
        systemLanguage = SystemLanguage.English;
        languageText = "";
        fontSize = 60;
    }

    public Jun_Language (SystemLanguage language,string text,int size)
    {
        systemLanguage = language;
        languageText = text;
        fontSize = size;
    }
}

[System.Serializable]
public class Jun_MultiLanguage 
{
    public delegate void OnSystemLanguageChange(SystemLanguage changeLanguage);

    public static OnSystemLanguageChange onSystemLanguageChange;
    private static bool m_autoLanguage = true;
    private static int m_showValue = 0;

    public static bool autoLanguage { get { return m_autoLanguage; } }
    public static int showValue { get { return m_showValue; } }

	public Jun_Language[] languages;

    public static void AutoLanguage (bool auto)
    {
        m_autoLanguage = auto;
        if (onSystemLanguageChange != null)
        {
            onSystemLanguageChange(Application.systemLanguage);
        }
        else
        {
            SetSystemLanguage((SystemLanguage)m_showValue);
        }
    }

    public static void SetSystemLanguage (SystemLanguage systemLanguage)
    {
        m_autoLanguage = false;
        m_showValue = (int)systemLanguage;

        if(onSystemLanguageChange != null)
        {
            onSystemLanguageChange(systemLanguage);
        }
    }

    public Jun_Language GetLanguage ()
    {
        
        if(m_autoLanguage)
            return GetLanguage(SystemLanguage.Korean);
        return GetLanguage((SystemLanguage)(m_showValue));
    }

    public Jun_Language GetLanguage(SystemLanguage systemLanguage)
    {
        bool haveLanguage = false;

        if (languages == null)
            return null;

        if (languages.Length <= 0)
            return null;

        foreach (Jun_Language thisLanguage in languages)
        {
            if (SameLanguages(thisLanguage.systemLanguage, systemLanguage))
            {
                haveLanguage = true;
                return thisLanguage;
            }
        }

        if (!haveLanguage)
        {
            if (languages.Length > 0)
            {
                return languages[0];
            }
        }

        return null;
    }

    public bool isChineseLanguage ()
    {
        return SameLanguages(Application.systemLanguage, SystemLanguage.Chinese);
    }

    public static bool SameLanguages (SystemLanguage language01,SystemLanguage language02)
	{
        if (language01 == SystemLanguage.Chinese && language02 == SystemLanguage.ChineseSimplified)
			return true;

        if (language01 == SystemLanguage.ChineseSimplified && language02 == SystemLanguage.Chinese)
			return true;

		if (language01 == language02)
			return true;

		return false;
    }

    public static bool SameLanguages(string language01, string language02)
    {
        if (language01 == SystemLanguage.Chinese.ToString() && language02 == SystemLanguage.ChineseSimplified.ToString())
			return true;

        if (language01 == SystemLanguage.ChineseSimplified.ToString() && language02 == SystemLanguage.Chinese.ToString())
			return true;

		if (language01 == language02)
			return true;

		return false;
    }

    public override string ToString()
    {
        Jun_Language currentLanguage = GetLanguage();
        if (currentLanguage != null)
            return currentLanguage.languageText;
        return "";
    }
}
