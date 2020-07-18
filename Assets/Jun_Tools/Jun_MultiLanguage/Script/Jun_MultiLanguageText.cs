using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Jun_MultiLanguageText : MonoBehaviour 
{
    public Jun_LanguageSettingType settingType = Jun_LanguageSettingType.Custom;

	public Jun_MultiLanguage multiLanguage = new Jun_MultiLanguage();
	TextMeshProUGUI uiText;

    [HideInInspector] [SerializeField] Jun_MultiLanguagePool m_languagePool;
    [HideInInspector] [SerializeField] string m_languageKey;
    [HideInInspector] [SerializeField] int m_languageID;

	// Use this for initialization
	void Start ()
	{
        ShowLanguage();
	}

    void ShowLanguage ()
    {
        uiText = GetComponent<TextMeshProUGUI>();

        if (uiText != null)
        {
            switch (settingType)
            {
                case Jun_LanguageSettingType.Custom:
                    Jun_Language thisLanguage = multiLanguage.GetLanguage();
                    if (thisLanguage != null)
                    {
                        uiText.SetLanguage(thisLanguage);
                    }
                    break;

                case Jun_LanguageSettingType.LanguagePool:
                    if (m_languagePool != null)
                    {
                        Jun_MultiLanguage thisML = m_languagePool.GetLanguage(m_languageKey);
                        if (thisML == null)
                            thisML = m_languagePool.GetLanguage(m_languageID);
                        if (thisML != null)
                        {
                            uiText.SetLanguage(thisML.GetLanguage());
                        }
                    }
                    break;
            }
        }
    }

    private void OnValidate()
    {
        ShowLanguage();
    }

    private void OnEnable()
    {
        Jun_MultiLanguage.onSystemLanguageChange += OnSystemLanguageChange;
    }

    private void OnDisable()
    {
        Jun_MultiLanguage.onSystemLanguageChange -= OnSystemLanguageChange;
    }

    void OnSystemLanguageChange (SystemLanguage systemLanguage)
    {
        ShowLanguage();
    }
}
