using UnityEngine;
using UnityEngine.UI;

public class LayoutPreferredChange : LayoutElement
{
    public RectTransform contentRect;
    public float maxHeight;

    override public float preferredHeight { get { return contentRect.sizeDelta.y > maxHeight ? maxHeight : contentRect.sizeDelta.y; } }
}
