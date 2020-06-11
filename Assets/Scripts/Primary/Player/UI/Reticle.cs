﻿using UnityEngine;
using TMPro;
using MLAPI;

public class Reticle : MonoBehaviour
{
    private RectTransform reticle;
    private int layerMask;
    private Vector3 pos;
    private Vector3 pos2;
    public GameObject reticleTip;
    public TextMeshProUGUI reticleText;
    private bool lookLoop = false;
    private Camera cam;
    private GameObject currentObj;
    private SelectedItemHandler selectedItemHandler;

    private void Start()
    {
        if (NetworkingManager.Singleton.IsClient) 
        {
            lookLoop = true;
            layerMask = LayerMask.GetMask("Clickable");
            reticle = GetComponent<RectTransform>();
            pos = new Vector3(0.063f, 0.063f, 0.063F);
            pos2 = new Vector3(0.08f, 0.08f, 0.08f);
            cam = Camera.main;
            selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
        }
    }


    private void FixedUpdate()
    {
        if(lookLoop) 
        {
            if (cam == null)
                return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 15, layerMask))
            {
                if (hit.collider != null)
                {
                    currentObj = hit.collider.gameObject;
                    reticle.localScale = pos2;
                    ShowTip();
                }
            }
            else if (reticle.localScale != pos || reticleTip.activeSelf)
            {
                reticle.localScale = pos;
                reticleTip.SetActive(false);
                currentObj = null;
            }
        }
    }

    public void Use() 
    {
        if(currentObj != null) 
        {
            PlayerActionManager.singleton.InteractWithClickable(currentObj.GetComponent<Clickable>().unique);
        }
        else 
        {
            if(selectedItemHandler == null) 
            {
                selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
                
            }
            selectedItemHandler.Use();
        }
    }


    private void ShowTip()
    {
        Clickable clickable = currentObj.GetComponent<Clickable>();
        if(clickable != null) 
        {
            string toolTip = clickable.toolTip;
            if (toolTip.Length > 0)
            {
                reticleTip.SetActive(true);
                reticleText.text = toolTip;
            }
        }
    }
}