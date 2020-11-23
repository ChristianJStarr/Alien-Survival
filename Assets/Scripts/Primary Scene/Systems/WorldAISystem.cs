using MLAPI;
using MLAPI.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldAISystem : MonoBehaviour
{
    private bool systemEnabled;// Is this System Enabled

    public static WorldAISystem Singleton;
    public PlayerCommandSystem playerCommandSystem;
    private int frameCount = 0;
    public List<AIControlObject> ai = new List<AIControlObject>();

    private ServerProperties serverProperties; //Properties for this Server
    [SerializeField] private GameObject[] enemyPrefabs; //All of the Enemy Prefabs
    [SerializeField] private GameObject[] friendlyPrefabs; //All the Friendly Prefabs
    private Vector3[] spawnpoints; //Spawnpoints for All Spawns

    //Configs
    public int c_EnemyAttackRadius = 80;
    public int c_EnemyWanderRadius = 800;
    public int c_RespawnTime = 600; //10 Minutes
    public int c_StateUpdateTime = 1; // 1 Second
    public int c_WalkSpeed = 1;
    public int c_RunSpeed = 5;



    private void Awake() { Singleton = this; }


    //START This System
    public bool StartSystem()
    {
        systemEnabled = true;

        //Gather Properties
        serverProperties = ServerConnect.singleton.GetServerProperties();
        if (serverProperties == null)
        {
            systemEnabled = false;
        }

        //Gather Spawnpoints
        if (!FindSpawnPoints()) 
        {
            systemEnabled = false;
        }

        //Check Prefabs
        if (enemyPrefabs.Length == 0 || friendlyPrefabs.Length == 0)
        {
            systemEnabled = false;
        }

        return systemEnabled;
    }

    //STOP This System
    public bool StopSystem()
    {
        systemEnabled = false;
        return true;
    }

    private void Update() 
    {
        if (systemEnabled) 
        {
            frameCount++;
            RunAILogic();
            if(frameCount == 20) 
            {
                frameCount = 0;
                UpdateAIState();
                KeepSpawnLevels();
            }
        }
    }







    //Find Spawn Points
    private bool FindSpawnPoints() 
    {
        List<Vector3> spawnpointList = new List<Vector3>();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("AISpawnpoint");
        for (int i = 0; i < temp.Length; i++)
        {
            spawnpointList.Add(temp[i].transform.position);
        }
        if (spawnpointList.Count > 0)
        {
            spawnpoints = spawnpointList.ToArray();
            return true;
        }
        else 
        {
            return false;
        }
    }

    //Primary AI Logic (Ran Once per Frame)
    private void RunAILogic() 
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i] != null)
            {
                if (ai[i].type == AI_Type.enemy) 
                {
                    RunEnemyLogic(ai[i]);
                }
                else if(ai[i].type == AI_Type.friendly)
                {
                    RunFriendlyLogic(ai[i]);
                }
            }
        }
    }
    
    //Logic for Enemy
    private void RunEnemyLogic(AIControlObject controlObject) 
    {
        if (controlObject.state == AI_State.attacking) //ATTACK LOGIC 
        {
            if (controlObject.playerAttackTarget != null && Vector3.Distance(controlObject.transform.position, controlObject.playerAttackTarget.transform.position) < 100)
            {
                if (controlObject.agent.hasPath)
                {
                    RaycastHit hit;
                    bool canSeeTarget = false;
                    if (Physics.Raycast(controlObject.transform.position, controlObject.playerAttackTarget.transform.position, out hit, 30))
                    {
                        canSeeTarget = (hit.collider.CompareTag("Player_Collider_Head") || hit.collider.CompareTag("Player_Collider_Body") || hit.collider.CompareTag("Player_Collider_Legs"));
                    }
                    if (canSeeTarget)
                    {
                        controlObject.agent.speed = 0.01f; //SLOW SLOW SLOW WALK
                                                           //Shoot here
                                                           //PEW PEW
                                                           //Stop Shooting here
                    }
                    else if (controlObject.agent.speed != c_RunSpeed)
                    {
                        controlObject.agent.speed = c_RunSpeed;
                    }
                }
                else if (!controlObject.agent.hasPath || (controlObject.agent.destination - controlObject.playerAttackTarget.transform.position).magnitude > 1)//Move towarads it
                {
                    controlObject.agent.SetDestination(controlObject.playerAttackTarget.transform.position);
                    controlObject.agent.speed = c_RunSpeed;
                }
            }
            else
            {
                controlObject.state = AI_State.guarding;
            }
        }
        else if (controlObject.state == AI_State.wandering) //WANDER LOGIC
        {
            if (!controlObject.agent.hasPath)
            {
                controlObject.agent.speed = c_WalkSpeed;
                float distance = Random.Range(20, 70);
                Vector3 randomDirection = Random.insideUnitSphere * distance;
                randomDirection += controlObject.transform.position;
                NavMeshHit navHit;
                NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
                controlObject.agent.SetDestination(navHit.position);
            }
        }
        else if (controlObject.state == AI_State.guarding) //GUARD LOGIC
        {
            controlObject.agent.speed = 0;
            controlObject.agent.isStopped = true;
        }
    }

    //Logic for Friendly
    private void RunFriendlyLogic(AIControlObject controlObject) 
    {
        if (controlObject.state == AI_State.wandering) //WANDER LOGIC
        {
            if (!controlObject.agent.hasPath)
            {
                controlObject.agent.speed = c_WalkSpeed;
                float distance = Random.Range(20, 70);
                Vector3 randomDirection = Random.insideUnitSphere * distance;
                randomDirection += controlObject.transform.position;
                NavMeshHit navHit;
                NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
                controlObject.agent.SetDestination(navHit.position);
            }
        }
        else if (controlObject.state == AI_State.guarding) //GUARD LOGIC
        {
            controlObject.agent.speed = 0;
        }
    }

    //Register AI Object
    public void RegisterAI(AIControlObject controlObject) 
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i].NetworkId == controlObject.NetworkId) 
            {
                ai[i] = controlObject;
                return;
            }
        }
        ai.Add(controlObject);
    }

    //Remove AI Object
    public void RemoveAI(ulong networkId) 
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i].NetworkId == networkId) 
            {
                ai.Remove(ai[i]);
                break;
            }
        }
    }

    //Update AI State LOOP
    private void UpdateAIState()
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i] != null)
            {
                int playersNearby = 0;
                PlayerControlObject attack = null;
                float currentLowestDistance = c_EnemyAttackRadius;

                foreach (PlayerControlObject controlObject in playerCommandSystem.players.Values)
                {
                    float distance = Vector3.Distance(controlObject.transform.position, ai[i].transform.position);
                    if (distance < c_EnemyAttackRadius && distance < currentLowestDistance)
                    {
                        currentLowestDistance = distance;
                        attack = controlObject;
                    }
                    else if (distance < c_EnemyWanderRadius)
                    {
                        playersNearby++;
                    }
                }
                if (attack != null)
                {
                    ai[i].playerAttackTarget = attack;
                    ai[i].state = AI_State.attacking;
                }
                else if (playersNearby > 0)
                {
                    ai[i].state = AI_State.wandering;
                }
                else
                {
                    ai[i].state = AI_State.guarding;
                }
            }
        }
    }

    //Keep Spawn Levels LOOP
    private void KeepSpawnLevels()
    {
        int enemy = serverProperties.maxEnemies;
        int friendly = serverProperties.maxFriendly;
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i] != null)
            {
                if (ai[i].type == AI_Type.enemy)
                {
                    enemy--;
                }
                else if (ai[i].type == AI_Type.friendly)
                {
                    friendly--;
                }
            }
        }
        for (int i = 0; i < enemy; i++)
        {
            SpawnEnemy();
        }
        for (int i = 0; i < friendly; i++)
        {
            SpawnFriendly();
        }
    }

    //Spawn A Single Enemy
    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length - 1)], spawnpoints[Random.Range(0, spawnpoints.Length - 1)], Quaternion.identity);

        NetworkedObject networkObject = enemy.GetComponent<NetworkedObject>();
        AIControlObject controlObject = enemy.GetComponent<AIControlObject>();
        if (networkObject != null && controlObject != null)
        {
            ai.Add(controlObject);
            networkObject.Spawn();
        }
        else
        {
            Destroy(enemy);
        }
    }

    //Spawn A Single Friendly
    private void SpawnFriendly()
    {
        GameObject friendly = Instantiate(friendlyPrefabs[Random.Range(0, friendlyPrefabs.Length - 1)], spawnpoints[Random.Range(0, spawnpoints.Length - 1)], Quaternion.identity);

        NetworkedObject networkObject = friendly.GetComponent<NetworkedObject>();
        AIControlObject controlObject = friendly.GetComponent<AIControlObject>();
        if (networkObject != null && controlObject != null)
        {
            ai.Add(controlObject);
            networkObject.Spawn();
        }
        else
        {
            Destroy(friendly);
        }
    }

}


public enum AI_State 
{
    attacking,
    wandering,
    guarding
}
public enum AI_Type
{
    friendly,
    enemy
}
