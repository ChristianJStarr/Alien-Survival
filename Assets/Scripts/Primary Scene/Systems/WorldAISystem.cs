using MLAPI;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

public class WorldAISystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled;// Is this System Enabled

    public static WorldAISystem Singleton;
    public PlayerObjectSystem playerObjectSystem;
    private int frameCount = 0;
    public List<AIControlObject> ai = new List<AIControlObject>();

    private ServerProperties serverProperties; //Properties for this Server
    [SerializeField] private GameObject[] enemyPrefabs; //All of the Enemy Prefabs
    [SerializeField] private GameObject[] friendlyPrefabs; //All the Friendly Prefabs
    private Vector3[] spawnpoints; //Spawnpoints for All Spawns

    //JOBS - State Change
    NativeArray<Vector3> playerLocations;
    NativeArray<Vector3> aiLocations;
    NativeArray<int> states;
    NativeArray<int> targets;
    JobHandle enemyStateChangeHandle;
    EnemyStateChange enemyStateChange;


    //Configs
    public int c_MaxFriendly = 0;
    public int c_MaxEnemies = 0;
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
        LoadServerProperties();

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

    //Load Server Properties
    private void LoadServerProperties() 
    {
        ServerProperties sp = ServerConnect.singleton.GetServerProperties();
        c_EnemyAttackRadius = sp.ai_EnemyAttackRadius;
        c_EnemyWanderRadius = sp.ai_EnemyWanderRadius;
        c_RespawnTime = sp.ai_RespawnTime;
        c_StateUpdateTime = sp.ai_StateUpdateTime;
        c_WalkSpeed = sp.ai_WalkSpeed;
        c_RunSpeed = sp.ai_RunSpeed;
    }

    public Snapshot_AI[] GetAIObjectsSnapshot() 
    {
        int count = ai.Count;
        Snapshot_AI[] instance = new Snapshot_AI[count];
        for (int i = 0; i < count; i++)
        {
            if (ai[i] == null) return new Snapshot_AI[0];
            instance[i] = ai[i].ConvertToSnapshot();
        }
        return instance;
    }


    //-------------------AI Functions--------------------
    public void DamageAI(ulong networkId, int amount) 
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (ai[i] != null && ai[i].NetworkId == networkId)
            {
                if(ai[i].health - amount > 0) 
                {
                    ai[i].health -= amount;
                }
                else 
                {
                    ai[i].health = 0;

                }
                break;
            }
        }
    }


    //-------------------AI Backend--------------------

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
            if (controlObject.playerAttackTarget != null)
            {
                if (controlObject.agent.hasPath)
                {
                    controlObject.agent.isStopped = false;
                    RaycastHit hit;
                    bool canSeeTarget = false;
                    if (Physics.Raycast(controlObject.transform.position, controlObject.playerAttackTarget.transform.position, out hit, 30))
                    {
                        canSeeTarget = (hit.collider.CompareTag("Player_Collider_Head") || hit.collider.CompareTag("Player_Collider_Body") || hit.collider.CompareTag("Player_Collider_Legs"));
                    }
                    if (canSeeTarget)
                    {
                        controlObject.agent.speed = 0.01f; 
                        //SLOW SLOW SLOW WALK
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
                controlObject.agent.isStopped = false;
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
  
    public static void Register(AIControlObject controlObject) 
    {
        if(Singleton != null) 
        {
            Singleton.RegisterAI(controlObject);
        }
    }
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
    public static void Remove(ulong networkId) 
    {
        if(Singleton != null) 
        {
            Singleton.RemoveAI(networkId);
        }
    }
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
        int aiCount = ai.Count;
        Vector3[] aiPositions = new Vector3[aiCount];
        for (int i = 0; i < aiCount; i++)
        {
            aiPositions[i] = ai[i].transform.position;
        }

        PlayerControlObject[] players = playerObjectSystem.GetAllPlayerControlObjects();

        int count = players.Length;
        Vector3[] playerPositions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            playerPositions[i] = players[i].transform.position;
        }

        playerLocations = new NativeArray<Vector3>(playerPositions, Allocator.TempJob);
        aiLocations = new NativeArray<Vector3>(aiPositions, Allocator.TempJob);
        states = new NativeArray<int>(aiCount, Allocator.TempJob);
        targets = new NativeArray<int>(aiCount, Allocator.TempJob);

        enemyStateChange = new EnemyStateChange();
        enemyStateChange.attack_radius = c_EnemyAttackRadius * c_EnemyAttackRadius;
        enemyStateChange.wander_radius = c_EnemyWanderRadius * c_EnemyWanderRadius;
        enemyStateChange.players = playerLocations;
        enemyStateChange.ai = aiLocations;
        enemyStateChange.state = states;
        enemyStateChange.targetIndex = targets;
        enemyStateChangeHandle = enemyStateChange.Schedule(aiCount, enemyStateChangeHandle);

        enemyStateChangeHandle.Complete();

        for (int i = 0; i < aiCount; i++)
        {
            if(states[i] == 0) 
            {
                ai[i].state = AI_State.guarding;
            }
            else if (states[i] == 1)
            {
                ai[i].state = AI_State.wandering;
            }
            else if (states[i] == 2 && ai[i].type == AI_Type.enemy)
            {
                ai[i].state = AI_State.attacking;
                ai[i].playerAttackTarget = players[targets[i] - 1];
            }
        }

        playerLocations.Dispose();
        aiLocations.Dispose();
        states.Dispose();
        targets.Dispose();

    }

    //Keep Spawn Levels LOOP
    private void KeepSpawnLevels()
    {
        int enemy = c_MaxEnemies;
        int friendly = c_MaxFriendly;
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
#endif
}


public struct EnemyStateChange : IJobFor
{
    [ReadOnly] public int attack_radius;
    [ReadOnly] public int wander_radius;
    [ReadOnly] public NativeArray<Vector3> players;
    [ReadOnly] public NativeArray<Vector3> ai;
    public NativeArray<int> state;
    public NativeArray<int> targetIndex;
    public void Execute(int i) 
    {
        int attackIndex = 0;
        int playersNearby = 0;
        float currentClosest = attack_radius;
        for (int x = 0; x < players.Length; x++)
        {
            float distance = (ai[i] - players[x]).sqrMagnitude;
            if (distance < attack_radius * attack_radius && distance < currentClosest * currentClosest)
            {
                currentClosest = distance;
                attackIndex = x + 1;
            }
            else if(distance < wander_radius) 
            {
                playersNearby++;
            }
        }

        if(attackIndex != 0) 
        {
            //State = Attack
            state[i] = 2;
            targetIndex[i] = attackIndex;
        }
        else if(playersNearby > 0) 
        {
            //State = Wander
            state[i] = 1;
        }
        else 
        {
            //State = Guard 3
            state[i] = 0;
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
