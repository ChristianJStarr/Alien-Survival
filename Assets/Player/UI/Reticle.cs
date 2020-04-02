using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public class Reticle : MonoBehaviour
{
    private RectTransform reticle;
    private int layerMask;
    private Vector3 pos;
    private Vector3 pos2;
    public GameObject reticleTip;
    public TextMeshProUGUI reticleText;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Clickable");
        reticle = this.GetComponent<RectTransform>();
        pos = new Vector3(0.063f, 0.063f, 0.063F);
        pos2 = new Vector3(0.08f, 0.08f, 0.08f);
    }
    void FixedUpdate()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 15, layerMask))
        {
            if (hit.collider != null)
            {
                reticle.localScale = pos2;
                ShowTip(hit.collider.gameObject);
            }
             
        }
        else if (reticle.localScale != pos || reticleTip.activeSelf)
        {
            reticle.localScale = pos;
            reticleTip.SetActive(false);
        }

    }

    private void ShowTip(GameObject obj)
    {
        string tip = obj.GetComponent<Persistant>().tip;
        if (tip.Length > 0)
        {
            reticleTip.SetActive(true);
            reticleText.text = tip;
        }

    }
}