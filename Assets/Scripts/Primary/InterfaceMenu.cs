using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceMenu : MonoBehaviour
{
    public int interfaceId = 0; // Interface Identifier
    public int interfaceType = 0; //Interface Type
    public string interfaceData; //Interface Data
    public bool enabledByDefault = true; // True for Enabled

    //Enable Interface
    public virtual void Enable(string data)
    { }

    //Disable Interface
    public virtual void Disable() 
    { }

    //Update Interfaces Data
    public virtual void UpdateData(string data)
    { }
}
