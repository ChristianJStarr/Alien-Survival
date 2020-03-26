using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class PlayerStats : ScriptableObject
{
    public string playerName = "";
    public int playerHealth = 0;
    public int playerExp = 0;
    public int playerWater = 0;
    public int playerFood = 0;
    public int playerCoins = 0;
    public float playerHours = 0.0F;
    public string playerInventory = "1";

}
