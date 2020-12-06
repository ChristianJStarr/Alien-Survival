using UnityEngine;
using TMPro;
using MLAPI;
using System.Collections;

public class UI_Reticle : MonoBehaviour
{
    public GameObject reticleTip; //Reticle Tooltip Object
    public TextMeshProUGUI reticleText; //Reticle Text
    public RectTransform reticle; //Reticle Dot

    private Camera cam; //Main Camera for Raycast
    private GameObject currentObj; //Current Object being looked at
    
    private int regularLayerMask; //Raycast LayerMask
    private bool raycast = false; //shoot raycasts?
    private bool showingError = false; //Showing red norify tip?


    //----Reticle Config----
    private Vector3 normal_scale = new Vector3(0.063f, 0.063f, 0.063F); //Scale of Reticle Dot
    private Vector3 large_scale = new Vector3(0.08f, 0.08f, 0.08f); //Enlarged Scale


    private void Start()
    {
        //Check if Client and Initialize
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            cam = Camera.main;
            regularLayerMask = LayerMask.GetMask("Clickable", "DeathDrop", "Resource");
            raycast = true;
        }
        else 
        {
            Destroy(this);
        }
    }


    //-----------------------------------------------------------------//
    //                            RAY CASTS                            //
    //-----------------------------------------------------------------//

    //Raycast Loop Check
    private void FixedUpdate()
    {
        if(raycast) 
        {
            ShootRegularRaycast();
        }
    }

    //Raycast: Shoot Regular
    private void ShootRegularRaycast() 
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 15, regularLayerMask))
        {
            if (hit.collider != null)
            {
                currentObj = hit.collider.gameObject;
                reticle.localScale = large_scale;
                ShowReticleTip(GetTooltip(), false);
            }
        }
        else if (reticle.localScale != normal_scale || reticleTip.activeSelf)
        {
            if (!showingError)
            {
                reticle.localScale = normal_scale;
                reticleTip.SetActive(false);
                currentObj = null;
            }
        }
    }


    //-----------------------------------------------------------------//
    //                      RETICLE TOOLTIP                            //
    //-----------------------------------------------------------------//

    //Show the reticle tooltip
    private void ShowReticleTip(string tip, bool red)
    {
        if (tip.Length == 0) return;
        if (showingError) showingError = false;
        if (red) 
        {
            showingError = true;
            reticleText.color = new Color32(255, 74, 74, 255);
            reticleText.text = tip;
            reticleTip.SetActive(true);
            StartCoroutine(ClearNotify());
        }
        else 
        {
            reticleText.text = tip;
            reticleText.color = new Color32(255, 255, 255, 255);
            reticleTip.SetActive(true);
        }
    }

    //Clear the Notification
    private IEnumerator ClearNotify()
    {
        yield return new WaitForSeconds(2);
        if (showingError)
        {
            showingError = false;
            reticleText.text = "";
            reticleTip.SetActive(false);
        }
    }

    //Get Tooltip String
    private string GetTooltip() 
    {
        ReticleTooltipObject tooltip = currentObj.GetComponent<ReticleTooltipObject>();
        if(tooltip != null) 
        {
            return tooltip.text;
        }
        return "";
    }
    


    public void Show() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
}



// Updated 12/4/2020