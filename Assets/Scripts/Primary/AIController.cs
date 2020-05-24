using MLAPI;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// AI Controller. Runs on server only. Handles All AI
/// </summary>
public class AIController : MonoBehaviour
{
    private Object[] enemies;
    private Object[] friendly;

    private bool spawnEnemies = true;
    private bool spawnFriendly = false;

    private int maxEnemies = 100;
    private int maxFriendly = 20;

    private List<GameObject> currentEnemies;
    private List<GameObject> currentFriendly;


    private void Start()
    {
       
        if (NetworkingManager.Singleton.IsServer) 
        {
            HandleAI();
        }
    }
    private void HandleAI() 
    {
        currentEnemies = new List<GameObject>();
        currentFriendly = new List<GameObject>();
        enemies = Resources.LoadAll("AI/Enemies");
        friendly = Resources.LoadAll("AI/Friendly");
        Debug.Log(enemies.Length);
        if (spawnEnemies) 
        {
            for (int i = 0; i < maxEnemies; i++)
            {
                SpawnEnemy();
            }
        }
        if (spawnFriendly) 
        {
            for (int i = 0; i < maxFriendly; i++)
            {
                SpawnFriendly();
            }
        }
    }



    private Vector3 RandomSpawnPoint() 
    {
        RaycastHit hit;
        int height = 200;
        int xmin = -300;
        int zmin = -500;
        int xmax = 1350;
        int zmax = 1200;

        Vector3 location = new Vector3(Random.Range(xmin, xmax), height, Random.Range(zmin, zmax));

        if (Physics.Raycast(location, Vector3.down, out hit, 300.0f))
        {
            return hit.point;
        }
        else
        {
            return RandomSpawnPoint();
        }
    }
    private void SpawnEnemy() 
    {
        GameObject enemy = enemies[Random.Range(0, enemies.Length)] as GameObject;
        GameObject liveEnemy = Instantiate(enemy, RandomSpawnPoint(), Quaternion.identity);
        liveEnemy.GetComponent<NetworkedObject>().Spawn();
        currentEnemies.Add(liveEnemy);
        
        Debug.Log("Network - Game - Spawned Enemy. Enemy Count: " + currentEnemies.Count);
    }
    private void DestroyEnemy(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy)) 
        {
            currentEnemies.Remove(enemy);
            Destroy(enemy);
            Debug.Log("Network - Game - Destroyed Enemy. Enemy Count: " + currentEnemies.Count);
        }
    }
    private void SpawnFriendly()
    {
        GameObject friend = friendly[Random.Range(0, enemies.Length)] as GameObject;
        GameObject liveFriend = Instantiate(friend, RandomSpawnPoint(), Quaternion.identity);
        liveFriend.GetComponent<NetworkedObject>().Spawn();
        currentFriendly.Add(liveFriend);
        Debug.Log("Network - Game - Spawned Friendly. Friendly Count: " + currentFriendly.Count);
    }
    private void DestroyFriendly(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
            Destroy(enemy);
            Debug.Log("Network - Game - Destroyed Friendly. Friendly Count: " + currentFriendly.Count);
        }
    }

}
