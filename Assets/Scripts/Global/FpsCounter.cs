using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    public float timer, refresh, avgFramerate;
    private Text countText;

    private void Start()
    {
        countText = GetComponent<Text>();
    }

    private void Update()
    {
        if (countText == null) return;
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            countText.text = fps.ToString();
            timer = Time.unscaledTime + refresh;
        }
    }
}