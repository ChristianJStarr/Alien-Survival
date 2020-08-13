using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCampfireFlicker : MonoBehaviour
{
    public Light fireLight;
    private bool flickerLight = true;
    private int intensity = 60;
    private bool flickerStarted;


    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;
    }
    private void OnDisable() 
    {
        SettingsMenu.ChangedSettings -= Change;
    }


    private void Start()
    {
        if(QualitySettings.GetQualityLevel() > 0) 
        {
            flickerStarted = true;
            StartCoroutine(StartFlicker());
        }
        else 
        {
            flickerStarted = false;
        }
    }

    private void Change()
    {
        if (!flickerStarted) 
        {
            if (QualitySettings.GetQualityLevel() > 0)
            {
                flickerStarted = true;
                flickerLight = true;
                StartCoroutine(StartFlicker());
            }
        }
        else if (QualitySettings.GetQualityLevel() == 0)
        {
            flickerStarted = false;
            flickerLight = false;
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
