using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MainMenuEasterEgg : MonoBehaviour
{
    public SkinnedMeshRenderer Alien;
    public Material GlowAlienMaterial;
    private Volume PostFx;
    private float weightTarget = 0;
    private float inc = 0.9F;
    private bool hasFaded = false;
    private bool fadeOut = false;

    private void Start()
    {
        PostFx = GetComponent<Volume>();
        weightTarget = 1;
        Material[] mats = Alien.materials;
        mats[1] = GlowAlienMaterial;
        Alien.materials = mats;
    }
    private void Update()
    {
        if (fadeOut) 
        {
            if (PostFx.weight != weightTarget)
            {
                PostFx.weight -= inc * Time.deltaTime;
            }
            if (PostFx.weight <= .5)
            {
                Alien.GetComponentInParent<Animator>().gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
        else 
        {
            if (PostFx.weight != weightTarget)
            {
                PostFx.weight += inc * Time.deltaTime;
            }
            if (!hasFaded)
            {
                if (PostFx.weight >= 1)
                {
                    hasFaded = true;
                    FadeAway();
                }
            }
        }
    }
    private void FadeAway() 
    {
        
        weightTarget = 0;
        fadeOut = true;
        
    }
}
