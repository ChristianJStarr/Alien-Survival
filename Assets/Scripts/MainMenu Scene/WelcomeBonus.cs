using System.Collections;
using UnityEngine;
using TMPro;
public class WelcomeBonus : MonoBehaviour
{
    public TextMeshProUGUI coin;
    public RectTransform icon;
    
    private float C_JumpHeight = 3F;
    private float C_JumpTime = 0.25F;
    private float C_Duration = 3;

    void Start()
    {
        StartCoroutine(CountBonus(1000, C_Duration, C_JumpHeight, C_JumpTime));
    }

    private IEnumerator CountBonus(int coins, float fill_delay, float jump_height, float jump_time) 
    {
        yield return new WaitForSeconds(1);
        fill_delay /= 2;
        float time = 0;   
        float half_time = jump_time / 2;
        float text_startPosition = coin.rectTransform.anchoredPosition.y;
        float icon_startPosition = icon.anchoredPosition.y;
        float text_target_height = text_startPosition + jump_height;
        float icon_target_height = icon_startPosition + jump_height;
        float stomp_time = 0;

        coin.text = "+" + 0;
        while (time < fill_delay)
        {
            coin.text = "+" + (int)Mathf.Lerp(0, coins, time / fill_delay);
            time += Time.deltaTime;
            yield return null;
        }
        coin.text = "+" + coins;

        
        while (stomp_time < jump_time)
        {
            coin.rectTransform.anchoredPosition = new Vector2(coin.rectTransform.anchoredPosition.x, Mathf.Lerp(text_startPosition, text_target_height, stomp_time / jump_time));
            icon.anchoredPosition = new Vector2(icon.anchoredPosition.x, Mathf.Lerp(icon_startPosition, icon_target_height, stomp_time / jump_time));
            stomp_time += Time.deltaTime;
            yield return null;
        }
        coin.rectTransform.anchoredPosition = new Vector2(coin.rectTransform.anchoredPosition.x, text_target_height);
        icon.anchoredPosition = new Vector2(icon.anchoredPosition.x, icon_target_height);
        stomp_time = 0;
        while (stomp_time < half_time)
        {
            coin.rectTransform.anchoredPosition = new Vector2(coin.rectTransform.anchoredPosition.x, Mathf.Lerp(text_target_height, text_startPosition, stomp_time / half_time));
            icon.anchoredPosition = new Vector2(icon.anchoredPosition.x, Mathf.Lerp(icon_target_height, icon_startPosition, stomp_time / half_time));
            stomp_time += Time.deltaTime;
            yield return null;
        }
        coin.rectTransform.anchoredPosition = new Vector2(coin.rectTransform.anchoredPosition.x, text_startPosition);
        icon.anchoredPosition = new Vector2(icon.anchoredPosition.x, icon_startPosition);
    }







}
