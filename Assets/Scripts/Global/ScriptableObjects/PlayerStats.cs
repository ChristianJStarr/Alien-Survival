using UnityEngine;
/// <summary>
/// Player Stats.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    //Persistant Data
    public string playerName = ""; //Player Username
    public int playerExp = 0; //Player Experience
    public int playerCoins = 0; //Server Coin Amount
    public float playerHours = 0.0F; //Hours Played

    //Server Specific Data
    public int playerHealth = 0; //Player Health
    public int playerWater = 0; //Player Water Level
    public int playerFood = 0; //Player Food Level
    public string playerInventory = ""; //Inventory Data
    public Vector3 location = new Vector3(0, 0, 0); //Stored Location

    //Wipe non-persistant data. Called when leaving server.
    public void Wipe() 
    {
        playerHealth = 0;
        playerFood = 0;
        playerWater = 0;
        playerInventory = "";
        location = new Vector3(0, 0, 0);
    }
}
