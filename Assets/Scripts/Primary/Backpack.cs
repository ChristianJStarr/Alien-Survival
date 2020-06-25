using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    public MeshRenderer glowRing;
    private int glowPosition = 0;

    public Material blueGlow; // Regular Glow 0
    public Material redGlow; // Red Glow 1
    public Material greenGlow; // Green Glow 2
    public Material yellowGlow; // Yellow Glow 3

    private PlayerInfoManager infoManager;

    private void Start()
    {
        infoManager = PlayerInfoManager.singleton;
        if(infoManager != null) 
        {
            infoManager.InitializeBackpackEffect(this);
        }
    }

    //Update the backpack Glow
    public void UpdateBackpackGlow(int value) 
    {
        Debug.Log("[Client] Backpack : Updating Backpack Glow.");
        if (value != glowPosition) 
        {
            glowPosition = value;
            if(value == 1) 
            {
                glowRing.materials[4] = redGlow;
            }
            if(value == 2) 
            {
                glowRing.materials[4] = greenGlow;
            }
            if(value == 3) 
            {
                glowRing.materials[4] = yellowGlow;
            }
            StartCoroutine(GlowFade());
        }
    }

    private IEnumerator GlowFade()
    {
        yield return new WaitForSeconds(1F);
        Debug.Log("[Client] Backpack : Reseting Backpack Glow.");
        glowRing.materials[4] = blueGlow;
        glowPosition = 0;
    }
}
