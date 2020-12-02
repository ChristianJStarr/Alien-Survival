using UnityEngine;
using UnityEngine.Rendering;

public class MainMenuEasterEgg : MonoBehaviour
{
    public GameObject Alien;
    private Volume PostFx;
    private float weightTarget = 0;
    private float inc = 0.9F;
    private bool hasFaded = false;
    private bool fadeOut = false;

    private void Start()
    {
        PostFx = GetComponent<Volume>();
        weightTarget = 1;
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
                Alien.SetActive(false);
                gameObject.SetActive(false);
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
                    weightTarget = 0;
                    fadeOut = true;
                }
            }
        }
    }
}


