using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //What: THE ULTIMATE PLAYER CONTROLLER
    //Where: Primary Scene / Network Player


    //Player Stats
    private int stat_Health;
    private int stat_Food;
    private int stat_Water;
    private int stat_Exp;









    private void Start()
    {
        if (RetrievePlayerStats()) 
        {
        
        }
    }


    //Store private stats to PlayerStats 

    private bool RetrievePlayerStats() 
    {
        

        return true;
    }

}
