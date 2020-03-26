using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolScript : MonoBehaviour
{
    private static int speed = 3;

    public InventoryScript inventoryScript;
    public Animator anim;
    //public ParticleSystem particles;
    public GameObject bulletImpact;
    public AudioSource sound_Shoot;

    Vector3 target;
    Vector3 side;
    Ray raycast;
    RaycastHit hit;
    
    bool isHit;
    bool isAiming = false;
    [HideInInspector] public bool isFiring = false;
    
    
    void Start()
    {
        anim = GetComponent<Animator>();
        //particles.GetComponent<ParticleSystem>();
        side = new Vector3(0.27f, -0.24f, 0.5549f) ;
        target = new Vector3(0f,-0.1f,0.56f);
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            AimPistol();
        }

        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            AimPistol();
        }

        if (Input.GetMouseButtonDown(0) && !isFiring)
        {
            isFiring = true;
            FirePistol();
        }
    }

    public void FirePistol() 
    {
        if (!inventoryScript.InOpen()) 
        {
            raycast = new Ray(transform.position, -transform.right * 100);
            if (Physics.Raycast(raycast, out hit, 100f))
            {
                Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
            }
            //particles.Play();
            sound_Shoot.Play();
            anim.SetBool("isFiring", true);
            StartCoroutine(ShootDelay());
        }
        else 
        {
        }
    }
    private void AimPistol()
    {
        float step = speed * Time.deltaTime;
        if (isAiming)
        {
            anim.SetBool("isAiming", true);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, step);
        }
        else if (!isAiming && transform.localPosition != side)
        {
            isAiming = false;
            anim.SetBool("isAiming", false);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, side, step);
        }
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("isFiring", false);
        isFiring = false;
    }
}
