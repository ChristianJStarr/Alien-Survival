using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCampfireFlicker : MonoBehaviour
{
    public Light fireLight;
    private bool flickerLight = true;
    private int intensity = 60;

    private void Start()
    {
        if(QualitySettings.GetQualityLevel() > 0) 
        {
            StartCoroutine(StartFlicker());
        }
    }

    private IEnumerator StartFlicker() 
    {
        WaitForSeconds wait = new WaitForSeconds(0.2F);
        while (flickerLight) 
        {
            yield return wait;
            intensity = Random.Range(50, 70);
        }
    }

    private void Update()
    {
        if(fireLight.intensity != intensity) 
        {
            fireLight.intensity = Mathf.Lerp(fireLight.intensity, intensity, 10 * Time.deltaTime);
        }        
    }
}
