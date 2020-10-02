using MLAPI;
using MLAPI.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISystem : MonoBehaviour
{
    private bool systemEnabled;// Is this System Enabled

    private List<AIObject> aiObjects = new List<AIObject>(); //A List of all AIObjects

    private ServerProperties serverProperties; //Properties for this Server
    [SerializeField] private GameObject[] enemyPrefabs; //All of the Enemy Prefabs
    [SerializeField] private GameObject[] friendlyPrefabs; //All the Friendly Prefabs
    private List<Vector3> spawnpoints = new List<Vector3>(); //Spawnpoints for All Spawns

    //START This System
    public bool StartSystem()
    {
        systemEnabled = true;

        //Gather Properties
        serverProperties = ServerConnect.singleton.GetServerProperties();
        if (serverProperties == null) 
        {
            return false;
        }

        //Gather Spawnpoints
        foreach (GameObject temp in GameObject.FindGameObjectsWithTag("AISpawnpoint"))
        {
            spawnpoints.Add(temp.transform.position);
        }
        if (spawnpoints.Count == 0) 
        {
            return false;
        }

        if(enemyPrefabs.Length == 0 || friendlyPrefabs.Length == 0) 
        {
            return false;
        }

        StartCoroutine(UpdateAIState());
        StartCoroutine(KeepSpawnLevels());


        return true;
    }

    //STOP This System
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }


   
    //Spawn A Single Enemy
    private void SpawnEnemy() 
    {
        GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length - 1)], spawnpoints[Random.Range(0, spawnpoints.Count - 1)], Quaternion.identity);
        aiObjects.Add(enemy.GetComponent<AIObject>());
        enemy.GetComponent<NetworkedObject>().Spawn();
    }

    //Spawn A Single Friendly
    private void SpawnFriendly() 
    {
        GameObject friendly = Instantiate(friendlyPrefabs[Random.Range(0, friendlyPrefabs.Length - 1)], spawnpoints[Random.Range(0, spawnpoints.Count - 1)], Quaternion.identity);
        aiObjects.Add(friendly.GetComponent<AIObject>());
        friendly.GetComponent<NetworkedObject>().Spawn();
    }

    //Update AI State LOOP
    private IEnumerator UpdateAIState() 
    {
        WaitForSeconds wait = new WaitForSeconds(1);
        while (systemEnabled) 
        {
            DebugMsg.Begin(97, "Setting AI States", 3);
            NetworkedClient[] connectedClients = NetworkingManager.Singleton.ConnectedClientsList.ToArray();
            for (int i = 0; i < aiObjects.Count; i++)
            {
                if (aiObjects[i] != null)
                {
                    int playersNearby = 0;
                    int playersAttack = 0;
                    Vector3 location = aiObjects[i].transform.position;
                    NetworkedObject attackPlayer = null;
                    for (int e = 0; e < connectedClients.Length; e++)
                    {
                        float distance = Vector3.Distance(location, connectedClients[e].PlayerObject.transform.position);
                        if (distance < 200 && distance > 80)
                        {
                            playersNearby++;
                        }
                        else if (aiObjects[i].type == AIObject.Type.enemy && distance <= 80)
                        {
                            attackPlayer = connectedClients[e].PlayerObject;
                            playersAttack++;
                        }
                    }
                    if(attackPlayer != null) 
                    {
                        Debug.DrawRay(location, attackPlayer.transform.position - location, Color.cyan, 1);
                    }


                    if (playersAttack > 0 && CanSeeTarget(location, attackPlayer.transform.position))
                    {
                        aiObjects[i].target = attackPlayer;
                        aiObjects[i].state = AIObject.State.attacking;
                    }
                    else if ((playersNearby > 0 || playersAttack > 0) && aiObjects[i].state != AIObject.State.attacking)
                    {
                        aiObjects[i].state = AIObject.State.wandering;
                    }
                    else
                    {
                        aiObjects[i].state = AIObject.State.guarding;
                    }
                }
            }
            DebugMsg.Begin(97, "Finished Setting AI States", 3);
            yield return wait;
        }
    }

    //Keep Spawn Levels LOOP
    private IEnumerator KeepSpawnLevels()
    {
        WaitForSeconds wait = new WaitForSeconds(600);
        while (systemEnabled)
        {
            int enemy = 0;
            int friendly = 0;
            for (int i = 0; i < aiObjects.Count; i++)
            {
                if (aiObjects[i] != null)
                {
                    if (aiObjects[i].type == AIObject.Type.friendly)
                        friendly++;
                    else if (aiObjects[i].type == AIObject.Type.enemy)
                        enemy++;
                }
            }

            enemy = serverProperties.maxEnemies - enemy;
            friendly = serverProperties.maxFriendly - friendly;

            if (enemy != 0)
            {
                DebugMsg.Notify("Keeping Spawn Levels: Spawning " + enemy + " Enemy(s)", 3);
                while (enemy > 0)
                {
                    SpawnEnemy();
                    enemy--;
                }
            }
            if (friendly != 0)
            {

                DebugMsg.Notify("Keeping Spawn Levels: Spawning " + friendly + " Friendly(s)", 3);
                while (friendly > 0)
                {
                    SpawnFriendly();
                    friendly--;
                }
            }
            yield return wait;
        }
    }



    private bool CanSeeTarget(Vector3 location, Vector3 target)
    {
        target = new Vector3(target.x, target.y + 2.5F, target.z); //Aim at chest
        RaycastHit hit;
        if(Physics.Raycast(location, target - location, out hit, 80)) 
        {
            return hit.collider.CompareTag("Player_Collider_Head") || hit.collider.CompareTag("Player_Collider_Body") || hit.collider.CompareTag("Player_Collider_Legs");
        }
        return false;
    }

}
