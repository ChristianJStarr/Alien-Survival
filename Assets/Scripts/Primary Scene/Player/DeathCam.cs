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
    private Vector3 move_target; //Move Target for Cam
    private Quaternion look_target; //Rotate Target for Cam
    private bool isActive = false; //Death Camera Active
    public int lerpSpeed = 5; //Speed of Camera Moving
    private int layerMask; //Player Headroom Ignore Mask
    

    private void Start() 
    {
        layerMask = ~LayerMask.GetMask("Player");
        deathCam = Instantiate(deathCamPrefab, transform.position, transform.rotation);
        deathCam.SetActive(false);
    }

    //Handle Camera Lerping
    private void FixedUpdate() 
    {
        if (isActive) 
        {
            if (deathCam == null) return;
            Vector3 offset = new Vector3(0, GetCeilingHeight(), 0);
            move_target = player.transform.position + offset;
            look_target = Quaternion.LookRotation(-(move_target - player.transform.position.normalized));
            if (move_target != deathCam.transform.position)
            {
                deathCam.transform.position = Vector3.Lerp(deathCam.transform.position, move_target, lerpSpeed * Time.deltaTime);
            }
            
            if (look_target != deathCam.transform.rotation)
            {
                deathCam.transform.rotation = Quaternion.Lerp(deathCam.transform.rotation, look_target, lerpSpeed * 2 * Time.deltaTime);
            }
        }
    }

    //Activate the Death Camera
    public void Activate(bool value)
    {
        animator.enabled = !value;
        if (value) 
        {
            deathCam.transform.position = transform.position;
            deathCam.transform.rotation = transform.rotation;
        }
        else 
        {
            animator.SetTrigger("Wake");
        }
        isActive = value;
        if (deathCam == null) return;
        deathCam.SetActive(value);
    }

    //Get Headroom of Player to Prevent Camera Clipping
    private float GetCeilingHeight() 
    {
        float offset = 4;
        RaycastHit hit;
        Vector3 startPosition = player.transform.position;
        if (Physics.Raycast(startPosition, Vector3.up, out hit, 10, layerMask)) 
        {
            if(hit.distance > 0.1) 
            {
                offset = hit.distance;
            }
        }
        return offset;
    }
}
