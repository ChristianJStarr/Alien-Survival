using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// AI Controller. Runs on server only. Handles All AI
/// </summary>
public class AIController : MonoBehaviour
{
    private Object[] enemies;
    private Object[] friendly;

    private int maxEnemies = 0;
    private int maxFriendly = 0;

    private List<GameObject> currentEnemies;
    private List<GameObject> currentFriendly;


    private void Start()
    {
       
        if (NetworkingManager.Singleton.IsServer) 
        {
            //maxEnemies = FindObjectOfType<ServerConnect>().maxEnemies;
            //maxFriendly = FindObjectOfType<ServerConnect>().maxFriendly;
            //HandleAI();
        }
    }
    private void HandleAI() 
    {
        currentEnemies = new List<GameObject>();
        currentFriendly = new List<GameObject>();
        enemies = Resources.LoadAll("AI/Enemies");
        friendly = Resources.LoadAll("AI/Friendly");
        SpawnOverTime(maxEnemies, maxFriendly);
    }

    private void SpawnOverTime(int maxEnemy, int maxFriend) 
    {
        if(maxEnemy > 0 || maxFriend > 0) 
        {
            StartCoroutine(SpawnOverTime_Cycle(maxEnemy, maxFriend));
        }
    }
    private IEnumerator SpawnOverTime_Cycle(int maxEnemy, int maxFriend)
    {

        int changedEnemy = 0;
        int changedFriend = 0;
        if (maxEnemy > 0)
        {
            SpawnEnemy();
            changedEnemy++;
        }
        if (maxFriend > 0)
        {
            SpawnFriendly();
            changedFriend++;
        }
        yield return new WaitForSeconds(.222f);
        SpawnOverTime(maxEnemy - changedEnemy, maxFriend - changedFriend);

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
        
    }
    private void DestroyEnemy(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy)) 
        {
            currentEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }
    private void SpawnFriendly()
    {
        GameObject friend = friendly[Random.Range(0, enemies.Length)] as GameObject;
        GameObject liveFriend = Instantiate(friend, RandomSpawnPoint(), Quaternion.identity);
        liveFriend.GetComponent<NetworkedObject>().Spawn();
        currentFriendly.Add(liveFriend);
    }
    private void DestroyFriendly(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }

}
