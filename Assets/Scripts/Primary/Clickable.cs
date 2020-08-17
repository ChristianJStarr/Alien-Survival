using MLAPI;
using MLAPI.NetworkedVar;

public class Clickable : NetworkedBehaviour
{
    [SyncedVar(SendTickrate = 0)]
    public string toolTip;
    [SyncedVar(SendTickrate = 0)]
    public string uniqueId;
    //Click Types
    // -1- Pickup Single
    // -2- Pickup Stack
    // -3- Inventory
    public int clickType;
    public int uiType;
    public string data;

}
