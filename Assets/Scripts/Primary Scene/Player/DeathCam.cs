﻿using System.Collections;
using UnityEngine;

public class DeathCam : MonoBehaviour
{
    #region Singleton
    public static DeathCam Singleton;

    private void Awake() { Singleton = this; }

    #endregion

    public Transform player; //Player Object Transform
    public GameObject deathCamPrefab; //Prefab for Deathcam
    public Animator animator;

    private GameObject deathCam; //Death Camera Object
    private int layerMask; //Player Headroom Ignore Mask
    
    private void Start() 
    {
        layerMask = ~LayerMask.GetMask("Player");
    }

    //Show the Camera
    public static void Show() 
    {
        if (Singleton != null) Singleton.Activate_Camera(true);
    }
    
    //Hide the Camera
    public static void Hide() 
    {
        if (Singleton != null) Singleton.Activate_Camera(false);
    }

    //Activate the Death Camera
    private void Activate_Camera(bool value)
    {
        if (value) 
        {
            deathCam.transform.position = transform.position;
            deathCam.transform.rotation = transform.rotation;
            if (deathCam == null)
            {
                deathCam = Instantiate(deathCamPrefab, transform.position, transform.rotation);
            }
            AlignCamera();
        }
        else if(deathCam != null)
        {
            deathCam.SetActive(false);
        }
        animator.enabled = !value;
    }

    //Align the Camera
    private void AlignCamera() 
    {
        deathCam.SetActive(true);
        Vector3 target_position = player.transform.position + new Vector3(0, GetCeilingHeight(), 0);
        StartCoroutine(LerpCamera(target_position, Quaternion.LookRotation(-(target_position - player.transform.position)), 10));
    }

    //Lerp the Camera Position & Rotation
    private IEnumerator LerpCamera(Vector3 position, Quaternion rotation, float duration) 
    {
        float time = 0;
        Vector3 startPosition = deathCam.transform.position;
        Quaternion startRotation = deathCam.transform.rotation;
        while (time < duration)
        {
            deathCam.transform.position = Vector3.Lerp(startPosition, position, time / duration);
            deathCam.transform.rotation = Quaternion.Lerp(startRotation, rotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        deathCam.transform.position = position;
        deathCam.transform.rotation = rotation;
    }

    //Get Headroom of Player to Prevent Camera Clipping
    private float GetCeilingHeight()
    {
        float offset = 4;
        RaycastHit hit;
        Vector3 startPosition = player.transform.position;
        if (Physics.Raycast(startPosition, Vector3.up, out hit, 10, layerMask))
        {
            if (hit.distance > 0.1)
            {
                offset = hit.distance;
            }
        }
        return offset;
    }


}
