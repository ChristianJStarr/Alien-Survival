using UnityEngine;
using TMPro;
using MLAPI;
using System.Collections;

public class Reticle : MonoBehaviour
{
    private RectTransform reticle;
    private int layerMask;
    private Vector3 pos;
    private Vector3 pos2;
    public GameObject reticleTip, reticleNotify;
    public TextMeshProUGUI reticleText, reticleNotifyText;


    private bool lookLoop = false;
    private Camera cam;
    private GameObject currentObj;
    private SelectedItemHandler selectedItemHandler;
    private PlayerActionManager playerActionManager;
    private bool showingError = false;
    
    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            playerActionManager = PlayerActionManager.singleton;
            lookLoop = true;
            layerMask = LayerMask.GetMask("Clickable", "DeathDrop", "Resource");
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
                    if (!showingError)
                    {
                        ShowTip();
                    }
                }
            }
            else if (reticle.localScale != pos || reticleTip.activeSelf)
            {
                if (!showingError) 
                {
                    reticle.localScale = pos;
                    reticleTip.SetActive(false);
                    currentObj = null;
                }
            }
        }
    }



    //Primary Use Function
    public void Use() 
    {
        if(currentObj != null) 
        {
            if(playerActionManager != null) 
            {
                Clickable clickable = currentObj.GetComponent<Clickable>();
                if(clickable != null) 
                {
                    playerActionManager.InteractWithClickable(clickable.unique);
                    return;
                }
                DeathDrop deathDrop = currentObj.GetComponent<DeathDrop>();
                if(deathDrop != null) 
                {
                    playerActionManager.InteractWithDeathDrop(deathDrop.unique, deathDrop.dropItems.ToArray());
                    return;
                }
                Resource resource = currentObj.GetComponent<Resource>();
                if (resource != null)
                {
                    playerActionManager.InteractWithResource(resource.unique);
                    return;
                }
            }
        }
        else 
        {
            if(selectedItemHandler == null) 
            {
                selectedItemHandler = FindObjectOfType<SelectedItemHandler>();
                
            }
            if (selectedItemHandler != null)
            {
                selectedItemHandler.Use();
            }
        }
    }

    //Show Reticle Tool Tip
    private void ShowTip()
    {
        Clickable clickable = currentObj.GetComponent<Clickable>();
        if (clickable != null)
        {
            string toolTip = clickable.toolTip;
            if (toolTip.Length > 0)
            {
                reticleTip.SetActive(true);
                reticleText.text = toolTip;
            }
        }
        DeathDrop deathDrop = currentObj.GetComponent<DeathDrop>();
        if (deathDrop != null)
        {
            string toolTip = deathDrop.toolTip;
            if (toolTip.Length > 0)
            {
                reticleTip.SetActive(true);
                reticleText.text = toolTip;
            }
        }
    }

    //Show Error Notift
    public void ShowErrorNotify(string notify) 
    {
        if (!showingError) 
        {
            showingError = true;
            reticleTip.SetActive(true);
            reticleText.text = notify;
            reticleText.color = new Color32(255, 74, 74, 255);
            StartCoroutine(ClearNotify());
        }
    }

    //Clear the Notification
    private IEnumerator ClearNotify() 
    {
        yield return new WaitForSeconds(1);
        showingError = false;
        reticleText.text = "";
        reticleText.color = new Color32(255, 255, 255, 255);
        reticleTip.SetActive(false);
    }

}