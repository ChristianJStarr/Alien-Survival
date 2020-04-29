using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Profile : MonoBehaviour
{
    public string userName;
    public float userHours;
    public int userSP;
    public int userXP;


    void Start() 
    {
        if (PlayerPrefs.GetString("nick") != "") 
        {
            userName = PlayerPrefs.GetString("nick");
        }
        else
        {
        }
        if (PlayerPrefs.GetFloat("hours") != 0)
        {
            userHours = PlayerPrefs.GetFloat("hours");
        }
        else 
        {
            userHours = 0.0f;
            PlayerPrefs.SetFloat("coins", 0.01F);
        }
        if (PlayerPrefs.GetInt("coins") != 0)
        {
            userSP = PlayerPrefs.GetInt("coins");
        }
        else 
        {
            userSP = 25;
                
            PlayerPrefs.SetInt("coins", userSP);
        }
    }
}
