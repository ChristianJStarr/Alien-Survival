using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RecentNews : MonoBehaviour
{
    public GameObject content;
    public GameObject slidePref;
    private string newsUrl = "https://www.game.aliensurvival.com/news.php";

    void Start()
    {
        StartCoroutine(GetNews());
    }

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
            Debug.Log(w.downloadHandler.text);
        }
    }
}
