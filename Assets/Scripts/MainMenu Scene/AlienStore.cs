using TMPro;
using UnityEngine;

public class AlienStore : MonoBehaviour
{

    public TextMeshProUGUI pointCount;
    public PlayerStats playerStats;
    private void Start()
    {
        pointCount.text = playerStats.playerCoins.ToString();
    }
}
