using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class AutoSave : MonoBehaviour
{
    public GameObject saveTile;
    public ObjectLoader objectLoader;
    private Animation anim;
    private TextMeshProUGUI text;
    public PlayerStats playerStats;
    private bool isSaving;
    private bool isMaster;
    int roomNumber;
    FirstPersonController player;
    

    void Start() 
    {
        player = FindObjectOfType<FirstPersonController>();
        isMaster = PhotonNetwork.IsMasterClient;
        anim = saveTile.GetComponent<Animation>();
        text = saveTile.GetComponentInChildren<TextMeshProUGUI>();
        string[] roomNameData = PhotonNetwork.CurrentRoom.Name.Split('#');
        roomNumber = Convert.ToInt32(roomNameData[1]);
        RunAutoSave();
    }
    IEnumerator AutoSaveLoop() 
    {
        yield return new WaitForSeconds(30);
        RunAutoSave();
        CloudPlayerStats();
    }
    void RunAutoSave() 
    {
        StartCoroutine(AutoSaveLoop());
    }

    public void AutoSaveOn() 
    {
        anim.Play("AutoSaveOn");
        text.text = "AUTO-SAVING...";
        StartCoroutine(OffWait());
    }

    IEnumerator OffWait() 
    {
        yield return new WaitForSeconds(2);
        AutoSaveOff();
    }

    public void AutoSaveOff() 
    {
        text.text = "COMPLETE";
        anim.Play("AutoSaveOff");
    }
    public void CloudPlayerStats()
    {
        if(player == null) 
        {
            player = FindObjectOfType<FirstPersonController>();
        }
        WWWForm form = new WWWForm();
        form.AddField("all", 2);
        form.AddField("userId", PlayerPrefs.GetInt("userId"));
        form.AddField("server", roomNumber);
        form.AddField("authKey", PlayerPrefs.GetString("authKey"));
        form.AddField("health", playerStats.playerHealth);
        form.AddField("water", playerStats.playerWater);
        form.AddField("food", playerStats.playerFood);
        form.AddField("inventory", playerStats.playerInventory);
        form.AddField("location", player.transform.position.ToString());
        UnityWebRequest w = UnityWebRequest.Post("https://www.game.aliensurvival.com/roomuser.php", form);
        StartCoroutine(SetStatsWait(w));
        
    }
    private IEnumerator SetStatsWait(UnityWebRequest _w)
    {
        isSaving = true;
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            isSaving = false;
        }
        else 
        {
            Debug.Log("Network - Failed Player Auto-Save");
        }
        if (isMaster)
        {
            WWWForm form = new WWWForm();
            form.AddField("json", objectLoader.AllObjectsToJson());
            form.AddField("server", roomNumber);
            form.AddField("all", 2);
            UnityWebRequest w = UnityWebRequest.Post("https://www.game.aliensurvival.com/roomworld.php", form);
            StartCoroutine(SetWorldWait(w));
        }
    }
    private IEnumerator SetWorldWait(UnityWebRequest _w)
    {
        isSaving = true;
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            isSaving = false;
        }
        else
        {
            Debug.Log("Network - Failed World Auto-Save");
        }
    }
    public void LocalPlayerStats() 
    {
        PlayerPrefs.SetString("recent", System.DateTime.Now.ToString());
        PlayerPrefs.SetInt("coins", playerStats.playerCoins);
        PlayerPrefs.SetFloat("hours", playerStats.playerHours);
        PlayerPrefs.SetInt("exp", playerStats.playerExp);
        PlayerPrefs.Save();
    }
    public void Save() 
    {
        if (!isSaving) 
        {
            CloudPlayerStats();
        }  
    }
}
