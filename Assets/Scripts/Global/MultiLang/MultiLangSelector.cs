using UnityEngine;

public class MultiLangSelector : MonoBehaviour
{
    public void ChangeLanguage(int language) 
    {
        MultiLangSystem.ChangeCurrentLanguage(language);
        Debug.Log("Changing Language to ID: " + language);       
    }
}
