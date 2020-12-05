using UnityEngine;

public class DeathCam : MonoBehaviour
{
    #region Singleton
    public static DeathCam Singleton;

    private void Awake() { Singleton = this; }

    #endregion

    public Transform player;
    public GameObject deathCamPrefab;
    private GameObject deathCam;

    private Vector3 move_target;
    private Quaternion look_target;
    private bool canLerp = false;
    public int lerpSpeed = 5;



    private void Start() 
    {
        deathCam = Instantiate(deathCamPrefab, transform.position, transform.rotation);
        deathCam.SetActive(false);
    }

    private void Update() 
    {
        if (canLerp) 
        {
            bool bothFinished = false;
            if(move_target != deathCam.transform.position) 
            {
                deathCam.transform.position = Vector3.Lerp(deathCam.transform.position, move_target, lerpSpeed * Time.deltaTime);
            }
            else { bothFinished = true; }
            
            
            if(look_target != deathCam.transform.rotation) 
            {
                deathCam.transform.rotation = Quaternion.Lerp(deathCam.transform.rotation, look_target, lerpSpeed * Time.deltaTime);
            }
            else if (bothFinished) { canLerp = false; }
        }
    }


    public void Activate(bool value)
    {
        if (value) 
        {
            Vector3 offset = new Vector3(0, 0, 0);
            offset.y = GetCeilingHeight();
            deathCam.transform.position = transform.position;
            deathCam.transform.rotation = transform.rotation;
            move_target = player.transform.position + offset;
            look_target = Quaternion.LookRotation(-((player.transform.position + offset) - player.transform.position).normalized);
            canLerp = true;
        }
        deathCam.SetActive(value);
    }

    private float GetCeilingHeight() 
    {
        float offset = 10;
        RaycastHit hit;
        Vector3 startPosition = player.transform.position + new Vector3(0, 1, 0);
        Debug.DrawLine(startPosition, player.transform.position + new Vector3(0,11,0));
        if (Physics.Raycast(startPosition, player.transform.up, out hit, 10)) 
        {
            Debug.Log("HIT    !!!        " + hit.distance);
            if(hit.distance > 1) 
            {
                offset = hit.distance;
            }
            else { offset = 5; }
        }
        return offset;
    }

}
