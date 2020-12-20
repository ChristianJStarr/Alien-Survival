using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MainMenuEasterEgg : MonoBehaviour
{
    public GameObject Alien;
    public float beamDuration = 2;
    private Volume PostFx;


    private void Start()
    {
        PostFx = GetComponent<Volume>();
        StartCoroutine(LerpPostFx());
    }

    private IEnumerator LerpPostFx() 
    {
        float time = 0;
        float startValue = PostFx.weight;
        while (time < beamDuration)
        {
            PostFx.weight = Mathf.Lerp(startValue, 1, time / beamDuration);
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;

        Alien.SetActive(false);
        gameObject.SetActive(false);

        while (time < beamDuration)
        {
            PostFx.weight = Mathf.Lerp(startValue, 0, time / beamDuration);
            time += Time.deltaTime;
            yield return null;
        }
        PostFx.weight = 0;
    }
}


