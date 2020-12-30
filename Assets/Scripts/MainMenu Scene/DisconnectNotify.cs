using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DisconnectNotify : MonoBehaviour
{
    public PlayerStats playerStats;
    public TextMeshProUGUI coinText, expText, timeText, serverName;
    public RectTransform coin_icon, exp_icon;

    private float C_FillTime = 3;
    private float C_JumpTime = 0.25F;
    private float C_JumpHeight = 3F;
    
    void Start()
    {
        if (playerStats.notifyData.Length > 0) 
        {
            SetCard(playerStats.notifyData);
            playerStats.notifyData = "";
        }
    }

    private void SetCard(string data) 
    {
        string[] datas = data.Split(',');
        serverName.text = datas[0];
        SetTimeText(datas[1]);
        StartCoroutine(AddEffect(Convert.ToInt32(datas[3]), Convert.ToInt32(datas[2]), C_FillTime, C_JumpTime, C_JumpHeight));
    }

    private void SetTimeText(string time) 
    {
        string t_hour = "HOUR";
        string t_hours = "HOURS";
        string t_minute = "MINUTE";
        string t_minutes = "MINUTES";
        string t_survived = "SURVIVED";

        float hours = float.Parse(time);
        int minutes = (int)TimeSpan.FromHours(hours).TotalMinutes;
        if(minutes >= 60) 
        {
            int hour = minutes / 60;
            int left = minutes - (hour * 60);
            if(left > 1) 
            {
                
                timeText.text = string.Format("{0} {1} {2} {3} {4}", hour, t_hour, left, t_minutes, t_survived);
            }
            else if(left == 1) 
            {
                timeText.text = string.Format("{0} {1} {2} {3} {4}", hour, t_hour, left, t_minute, t_survived);

            }
            else if(hour == 1)
            {
                timeText.text = string.Format("1 {0} {1}", t_hour, t_survived);
            }
            else
            {
                timeText.text = string.Format("{0} {1} {2}", hour, t_hours, t_survived);
            }
        }
        else 
        {
            timeText.text = string.Format("{0} {1} {2}", minutes, t_minutes, t_survived);
        }
    }

    private IEnumerator AddEffect(int coins, int exp, float fill_delay, float jump_time, float jump_height)
    {
        bool doCounting = true;
        bool doStomp = true;

        fill_delay /= 2;
        int current_coins = 0;
        int current_exp = 0;
        coinText.text = "+" + 0;
        expText.text = "+" + 0;

        float time = 0;
        float stomp_time = 0;
        float half_time = jump_time / 2;
        float text_startPosition;
        float icon_startPosition;
        float xTime = Time.deltaTime;
        float target_height = 0;

        if (doCounting)
        {
            while (time < fill_delay)
            {
                current_coins = (int)Mathf.Lerp(0, coins, time / fill_delay);
                coinText.text = "+" + current_coins;
                time += Time.deltaTime;
                yield return null;
            }
        }

        current_coins = coins;
        coinText.text = "+" + current_coins;

        //STOMP
        if (doStomp)
        {
            text_startPosition = coinText.rectTransform.anchoredPosition.y;
            icon_startPosition = coin_icon.anchoredPosition.y;
            target_height = text_startPosition + jump_height;
            stomp_time = 0;
            while (stomp_time < jump_time)
            {
                coinText.rectTransform.anchoredPosition = new Vector2(coinText.rectTransform.anchoredPosition.x, Mathf.Lerp(text_startPosition, target_height, stomp_time / jump_time));
                coin_icon.anchoredPosition = new Vector2(coin_icon.anchoredPosition.x, Mathf.Lerp(icon_startPosition, target_height, stomp_time / jump_time));
                stomp_time += xTime;
                yield return null;
            }
            coinText.rectTransform.anchoredPosition = new Vector2(coinText.rectTransform.anchoredPosition.x, target_height);
            coin_icon.anchoredPosition = new Vector2(coin_icon.anchoredPosition.x, target_height);

            stomp_time = 0;
            while (stomp_time < half_time)
            {
                coinText.rectTransform.anchoredPosition = new Vector2(coinText.rectTransform.anchoredPosition.x, Mathf.Lerp(target_height, text_startPosition, stomp_time / half_time));
                coin_icon.anchoredPosition = new Vector2(coin_icon.anchoredPosition.x, Mathf.Lerp(target_height, icon_startPosition, stomp_time / half_time));
                stomp_time += xTime;
                yield return null;
            }

            coinText.rectTransform.anchoredPosition = new Vector2(coinText.rectTransform.anchoredPosition.x, text_startPosition);
            coin_icon.anchoredPosition = new Vector2(coin_icon.anchoredPosition.x, icon_startPosition);
        }


        if (doCounting)
        {
            time = 0;
            while (time < fill_delay)
            {
                current_exp = (int)Mathf.Lerp(0, exp, time / fill_delay);
                expText.text = "+" + current_exp;
                time += Time.deltaTime;
                yield return null;
            }
        }

        current_exp = exp;
        expText.text = "+" + current_exp;


        if (doStomp)
        {
            text_startPosition = expText.rectTransform.anchoredPosition.y;
            icon_startPosition = exp_icon.anchoredPosition.y;
            target_height = text_startPosition + jump_height;
            stomp_time = 0;
            while (stomp_time < jump_time)
            {
                expText.rectTransform.anchoredPosition = new Vector2(expText.rectTransform.anchoredPosition.x, Mathf.Lerp(text_startPosition, target_height, stomp_time / jump_time));
                exp_icon.anchoredPosition = new Vector2(exp_icon.anchoredPosition.x, Mathf.Lerp(icon_startPosition, target_height, stomp_time / jump_time));
                stomp_time += xTime;
                yield return null;
            }
            expText.rectTransform.anchoredPosition = new Vector2(expText.rectTransform.anchoredPosition.x, target_height);
            exp_icon.anchoredPosition = new Vector2(exp_icon.anchoredPosition.x, target_height);

            stomp_time = 0;
            while (stomp_time < half_time)
            {
                expText.rectTransform.anchoredPosition = new Vector2(expText.rectTransform.anchoredPosition.x, Mathf.Lerp(target_height, text_startPosition, stomp_time / half_time));
                exp_icon.anchoredPosition = new Vector2(exp_icon.anchoredPosition.x, Mathf.Lerp(target_height, icon_startPosition, stomp_time / half_time));
                stomp_time += xTime;
                yield return null;
            }

            expText.rectTransform.anchoredPosition = new Vector2(expText.rectTransform.anchoredPosition.x, text_startPosition);
            exp_icon.anchoredPosition = new Vector2(exp_icon.anchoredPosition.x, icon_startPosition);
        }
    }
}
