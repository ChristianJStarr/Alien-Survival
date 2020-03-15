using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ServerCoins : MonoBehaviour
{
    public int coinAmount;
    public GameObject coinNotify;
    public TextMeshProUGUI coinText;

    private static ServerCoins serverCoins;

    public static ServerCoins Instance() 
    {
        if (!serverCoins)
        {
            serverCoins = FindObjectOfType(typeof(ServerCoins)) as ServerCoins;
        }
        return serverCoins;
    }

    void Start() 
    {
        coinText.text = coinAmount + " SP";
    }


    public void AddCoin(int value) 
    {
        coinAmount += value;
        coinText.text = coinAmount + " SP";
    }
    public void NotEnoughCoin(bool value) 
    {
        coinNotify.SetActive(value);
    }
    public bool   RemoveCoin(int value) 
    {
        if (coinAmount >= value) 
        {
            coinAmount -= value;
            coinText.text = coinAmount + " SP";
            return true;
        }
        else
        {
            NotEnoughCoin(true);
            return false;
        }
    }




}
