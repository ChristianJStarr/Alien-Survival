using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueExample : MonoBehaviour {
    
    public Jun_MultiLanguage[] languages;
    public TextMeshProUGUI text;

    private Jun_MultiLanguage currentLanguage;

	// Use this for initialization
    IEnumerator Start () 
    {
        for (int i = 0; i < languages.Length; i++)
        {
            currentLanguage = languages[i];
            text.SetLanguage(currentLanguage.GetLanguage());
            yield return new WaitForSeconds(2);

            if (i == languages.Length - 1)
                i = -1;
        }
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
        if(currentLanguage != null)
            text.SetLanguage(currentLanguage.GetLanguage());
    }
}
