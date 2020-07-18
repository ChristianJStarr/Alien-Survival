using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Jun_PopupWindow : EditorWindow 
{
    public static T Show<T>() where T:EditorWindow
    {
        T popup = ScriptableObject.CreateInstance<T>();
        popup.ShowPopup();
        popup.Focus();
        return popup;
    }

    protected bool isShowed = false;

    public virtual void OnGUI ()
    {
        if(!isShowed)
        {
            isShowed = true;
            Event curEvent = Event.current;
            Vector2 mousePosition = curEvent.mousePosition;
            position = new Rect(mousePosition.x + position.x, mousePosition.y + position.y, position.width, position.height);
        }
    }
}
