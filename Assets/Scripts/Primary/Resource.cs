using MLAPI;
using MLAPI.NetworkedVar;

public class Resource : NetworkedBehaviour
{ 
    [SyncedVar(SendTickrate = 0)]
    public string uniqueId = "";
    [SyncedVar(SendTickrate = 0)]
    public int gatherItemId = 0;
    public int gatherAmount = 1;
    public  int gatherPerAmount = 1;
}
