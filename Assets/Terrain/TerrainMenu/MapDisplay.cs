using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    public RawImage uiImage;

    public void DrawTexture(Texture2D texture)  
    {
        uiImage.texture = texture;
    }
}
