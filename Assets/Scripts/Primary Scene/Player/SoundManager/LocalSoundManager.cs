using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalSoundManager : NetworkedBehaviour
{
    //The Local Sound Manger

    public static LocalSoundManager Singleton;
    [SerializeField]private int poolSize = 20;

    //Counting for stats
    private int currentUsed = 0;
    private int maxUsed = 0;
    private int averageSum = 0;
    private int averageTimes = 0;
    private int average = 0;

    //Audio Sources Pool
    private Queue<AudioSource> pool;

    //Server Distance Dictionary
    private Dictionary<string, int> distanceDict = new Dictionary<string, int>();

    //Last AudioClip
    private LocalSoundEffect lastEffect;
    
    //NetworkingManager
    private NetworkingManager networkingManager;


    void Awake()
    {
        networkingManager = NetworkingManager.Singleton;
        Singleton = this;
        pool = new Queue<AudioSource>();
        if (IsClient) 
        {
            for (int i = 0; i < poolSize; i++)
            {
                pool.Enqueue(CreateNewInstance());
            }
        }
    }


    //Create Audio Source Instance
    private AudioSource CreateNewInstance()
    {
        GameObject go = new GameObject("AudioSourceInstance");
        go.transform.parent = this.transform;
        currentUsed++;
        averageTimes++;
        if(currentUsed > maxUsed) 
        {
            maxUsed = currentUsed;
        }
        averageSum += currentUsed;
        average = averageSum / averageTimes;

        DebugMsg.Notify("LocalAudio Instances, CURRENT:" + currentUsed + " MAX:" + maxUsed + " AVERAGE:" + average, 3);
        return go.AddComponent<AudioSource>();
    }

    //Get an Audio Source from the Pool or Create new Instance
    public AudioSource GetAudioSource()
    {
        if (pool.Count < 1)
        {
            return CreateNewInstance();
        }
        else
        {
            return pool.Dequeue();
        }
    }


    //Server RPC. Calculate who should hear this sound
    public void PlaySound(string soundName, Vector3 location, ulong excludeClient = 0)
    {
        if (IsServer) 
        {
            using (PooledBitStream writeStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
                {
                    writer.WriteVector3Packed(location);
                    writer.WriteStringPacked(soundName);
                    int distance = 0;
                    
                    if(distanceDict.Count > 0 && distanceDict.ContainsKey(soundName)) 
                    {
                        distance = distanceDict[soundName];
                    }
                    else 
                    {
                        distance = (Resources.Load<LocalSoundEffect>("Sounds/" + soundName)).distance;
                        distanceDict.Add(soundName, distance);
                    }
                    foreach (ulong client in networkingManager.ConnectedClients.Keys.ToArray())
                    {
                        if (excludeClient == 0 && client != excludeClient && Vector3.Distance(location, networkingManager.ConnectedClients[client].PlayerObject.transform.position) < distance)
                        {
                            InvokeClientRpcOnClientPerformance(Client_SoundCatch, client, writeStream);
                        }
                    }
                }
            }
        }
        else 
        {
            using (PooledBitStream writeStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
                {
                    writer.WriteVector3Packed(location);
                    writer.WriteStringPacked(soundName);

                    InvokeServerRpcPerformance(Server_PlaySound, writeStream);

                    PlayLocalSound(soundName, location);                    
                }
            }
        }
    }

    
    [ServerRPC(RequireOwnership = false)]
    private void Server_PlaySound(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            Vector3 location = reader.ReadVector3Packed();
            PlaySound(reader.ReadStringPacked().ToString(), location, clientId);
        }
    }

    //The client catch from the server to play a clip
    [ClientRPC]
    private void Client_SoundCatch(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            Vector3 location = reader.ReadVector3Packed();
            PlayLocalSound(reader.ReadStringPacked().ToString(), location);
        }
    }
    

    private void PlayLocalSound(string soundName, Vector3 location) 
    {
        AudioSource source = GetAudioSource();
        if (source != null)
        {
            source.transform.position = location;
            string clipName = soundName;
            LocalSoundEffect soundEffect = null;
            if (lastEffect != null && lastEffect.name == clipName)
            {
                soundEffect = lastEffect;
            }
            else
            {
                soundEffect = Resources.Load<LocalSoundEffect>("Sounds/" + clipName);
            }
            if (soundEffect != null)
            {
                AudioClip clip = null;
                source.maxDistance = soundEffect.distance;
                if (soundEffect.audioClips.Length > 1)
                {
                    int random = Random.Range(0, soundEffect.audioClips.Length - 1);
                    if (soundEffect.lastPlayedIndex != 99 && soundEffect.lastPlayedIndex == random)
                    {
                        random = Random.Range(0, soundEffect.audioClips.Length - 1);
                        if (soundEffect.lastPlayedIndex == random)
                        {
                            random = Random.Range(0, soundEffect.audioClips.Length - 1);
                        }
                    }
                    soundEffect.lastPlayedIndex = random;
                    clip = soundEffect.audioClips[random];
                }
                else
                {
                    clip = soundEffect.audioClips[0];
                }
                if (clip != null)
                {
                    source.PlayOneShot(clip, soundEffect.volume);
                    StartCoroutine(WaitForFinish(source, clip.length));
                    return;
                }
            }
        }
        ReturnAudioSource(source);
    }



    //Wait for the clip to finish and the return it
    private IEnumerator WaitForFinish(AudioSource source, float length) 
    {
        yield return new WaitForSeconds(length);
        ReturnAudioSource(source);
    }

    //Return instance to pool or destroy it if pool is full
    public void ReturnAudioSource(AudioSource instance) 
    {
        if(currentUsed < poolSize) 
        {
            pool.Enqueue(instance);
        }
        else 
        {
            Destroy(instance.gameObject);
            currentUsed--;
        }
    }



}
