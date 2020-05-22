using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using UnityEngine;

public class GameServer : MonoBehaviour
{
   

    /// <summary>
    /// Start Client & Connect to Server.
    /// </summary>
    /// <param name="ip">Server IP Address</param>
    /// <param name="port">Server Port</param>
    public void ConnectToServer(string ip, ushort port)
    {
        NetworkingManager nw = NetworkingManager.Singleton;
        nw.GetComponent<UnetTransport>().ConnectAddress = ip;
        nw.GetComponent<UnetTransport>().ConnectPort = port;
        nw.StartClient();
    }
    /// <summary>
    /// Start Server as Server
    /// </summary>
    public void StartServer()
    {
        NetworkingManager nw = NetworkingManager.Singleton;
        nw.ConnectionApprovalCallback += ApprovalCheck;
        nw.StartHost();
    }

    private void ConnectSuccessful(System.Action<ulong> callback=null) 
    {
    
    }
    /// <summary>
    /// Approval Check Callback.
    /// </summary>
    /// <param name="connectionData"></param>
    /// <param name="clientId"></param>
    /// <param name="callback"></param>
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback) 
    {
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;

        ulong? prefabHash = SpawnManager.GetPrefabHashFromGenerator(null); // The prefab hash. Use null to use the default player prefab
        //Find array of available spawn points.
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        //Pick a random point in array of available.
        Vector3 spawnPoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position; 
        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, prefabHash, approve, spawnPoint, Quaternion.identity);
    }
}
