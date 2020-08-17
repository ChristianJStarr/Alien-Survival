using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public MeshRenderer meshRenderer;


    //Toggle Material of Placeable - Glow / Orgingal Material
    public void ChangeMaterial(Material outlineMaterial) 
    {
        if (outlineMaterial != null) 
        {
            if(meshRenderer.sharedMaterial != outlineMaterial)
            {
                meshRenderer.sharedMaterial = outlineMaterial;
            }
        }
    }
}
