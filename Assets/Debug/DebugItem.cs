using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugItem : MonoBehaviour
{
    public Image image;
    public Item item;
    public Button button;
    public DebugHandler debugHandler;

   public void SetType(Sprite sImage, Item sItem) 
   {
        image.sprite = sImage;
        item = sItem;
   }
   public void Spawn() 
   {
        debugHandler.Item(item);
   }
}
