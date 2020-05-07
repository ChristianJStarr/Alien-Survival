using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Photon.Pun;


public class LoadPlayer : MonoBehaviour
{

    public Inventory inv;
    public PlayerStats playerStats;
    private ObjectLoader objectLoader;
    public LoadAwake loadAwake;
    private readonly string url = "https://www.game.aliensurvival.com";


    void Start()
    {
        objectLoader = GetComponent<ObjectLoader>();
        string[] roomNameData = PhotonNetwork.CurrentRoom.Name.Split('#');
        GetPlayerData(Convert.ToInt32(roomNameData[1]));
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GetWorldData(Convert.ToInt32(roomNameData[1])));
        }
    }

    private void GetPlayerData(int id)
    {
        WWWForm form = new WWWForm();
        form.AddField("all", 1);
        form.AddField("userId", PlayerPrefs.GetInt("userId"));
        form.AddField("authKey", PlayerPrefs.GetString("authKey"));
        form.AddField("server", id);
        UnityWebRequest w = UnityWebRequest.Post(url + "/roomuser.php", form);
        StartCoroutine(PlayerDataWait(w));
    }
    private IEnumerator PlayerDataWait(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] floatData = _w.downloadHandler.text.Split('!');
            string inventory = floatData[1];
            int health = Convert.ToInt32(floatData[2]);
            int food = Convert.ToInt32(floatData[3]);
            int water = Convert.ToInt32(floatData[4]);
            if (floatData[5].Length == 0)
            {
                floatData[5] = "(0,0,0)";
            }
            Vector3 location = StringToVector3(floatData[5]);
            playerStats.Wipe();
            playerStats.playerInventory = inventory;
            playerStats.playerHealth = health;
            playerStats.playerFood = food;
            playerStats.playerWater = water;
            playerStats.location = location;
            inv.GetInventory();
            Transform spawn = GameObject.Find("SpawnPoint").transform;
            if (playerStats.location != new Vector3(0, 0, 0))
            {
                spawn.position = playerStats.location;
            }
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkPlayer"), spawn.position, Quaternion.identity, 0);
            if (!PhotonNetwork.IsMasterClient)
            {
                ReadyToWakeUp();//Make sleeping screen ready to wake up.
            }
        }
        else
        {
            Debug.Log("Network - Failed to get Player Data  : " + _w.downloadHandler.text);
            SceneManager.LoadScene(1);
        }
    }

    public void ReadyToWakeUp() 
    {
        loadAwake.ReadyWake();
    }

    private static Vector3 StringToVector3(string sVector)
    {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }
        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
        return result;
    }


    private IEnumerator GetWorldData(int id)
    {
        WWWForm form = new WWWForm();
        form.AddField("all", 1);
        form.AddField("server", id);
        UnityWebRequest w = UnityWebRequest.Post(url + "/roomworld.php", form);
        yield return w.SendWebRequest();
        if (w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] downloadData = w.downloadHandler.text.Split('!');
            string worldData = downloadData[1];
            if (worldData.Length > 2)
            {
                objectLoader.JsonToAllObjects(worldData);
            }
            else
            {
                //Network - No World Data to Load.
            }
        }
        else
        {
            SceneManager.LoadScene(1);
            //Network - Error getting world data.
        }
    }
}
