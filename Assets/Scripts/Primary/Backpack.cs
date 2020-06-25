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

    private List<int> pendingGlows;

    private void Start()
    {
        pendingGlows = new List<int>();
        infoManager = PlayerInfoManager.singleton;
        if(infoManager != null) 
        {
            infoManager.InitializeBackpackEffect(this);
        }
    }

    //Update the backpack Glow
    public void UpdateBackpackGlow(int value) 
    {
        if (glowPosition == 0) 
        {
            glowPosition = value;
            if(value == 1) 
            {
                SetMaterial(redGlow);
            }
            if(value == 2) 
            {
                SetMaterial(greenGlow);
            }
            if(value == 3) 
            {
                SetMaterial(yellowGlow);
            }
            StartCoroutine(GlowFade());
        }
        else if(glowPosition == value)
        {
            pendingGlows.Add(value);
        }
    }

    private IEnumerator GlowFade()
    {
        WaitForSeconds wait = new WaitForSeconds(1F);
        yield return wait;
        if (pendingGlows.Contains(glowPosition))
        {
            for (int i = 0; i < pendingGlows.Count; i++)
            {
                if(pendingGlows[i] == glowPosition) 
                {
                    yield return wait;
                }
            }
        }
        SetMaterial(blueGlow);
        glowPosition = 0;
        pendingGlows.Clear();
    }


    private void SetMaterial(Material material) 
    {
        Material[] mats = glowRing.materials;
        mats[4] = material;
        glowRing.materials = mats;
    }
}
