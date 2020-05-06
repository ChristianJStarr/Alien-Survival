using Photon.Pun;
using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{

    public PhotonView photonView;

    void Start() 
    {
        TextMeshProUGUI text = this.GetComponentInChildren<TextMeshProUGUI>();
        if (photonView != null) 
        {
            if (photonView.IsMine)
            {
                Destroy(this.gameObject);
            }
            else
            {
                if (photonView.Owner.NickName != null)
                {
                    text.text = photonView.Owner.NickName;
                }
            }
        }
    }
  
    void Update()
    {
        if(Camera.main != null) 
        {
            this.transform.forward = Camera.main.transform.position - this.transform.position;
        }
    }
}
