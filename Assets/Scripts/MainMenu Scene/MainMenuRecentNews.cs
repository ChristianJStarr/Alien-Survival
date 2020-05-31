using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// Handle Devblog Display on MainMenu
/// </summary>
public class MainMenuRecentNews : MonoBehaviour
{

    public GameObject content;
    public GameObject slidePref;
    private string newsUrl = "https://www.game.aliensurvival.com/news.php";

    void Start()
    {
        StartCoroutine(GetNews());
    }


    /// <summary>
    /// Get the news and instantiate slides.
    /// </summary>
    /// <returns>Coroutine</returns>
    public IEnumerator GetNews() 
    {
        WWWForm form = new WWWForm();
        form.AddField("all", 1);
        UnityWebRequest w = UnityWebRequest.Post(newsUrl, form);
        yield return w.SendWebRequest();
        if (w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] data = w.downloadHandler.text.Split('*');
            data = data[1].Split('@');
            foreach (string dat in data)
            {
                if (String.IsNullOrEmpty(dat)) { break; }
                string[] datas = dat.Split('#');
                GameObject slide = Instantiate(slidePref, content.transform);
                slide.GetComponent<BlogSlide>().Set(datas[0], datas[1]);
            }
        }
        else
        {
            Debug.Log("Network - Web - Unable to get recent news.");
        }
    }
}
