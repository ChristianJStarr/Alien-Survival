using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [SerializeField]
    private Dictionary<string, LocalSoundEffect> effects = new Dictionary<string, LocalSoundEffect>();
    
    //NetworkingManager
    private NetworkingManager networkingManager;


#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] guids = AssetDatabase.FindAssets("t:LocalSoundEffect", new[] { "Assets/Content/LocalSoundEffects" });
        int count = guids.Length;
        if (effects.Count == count) return;
        for (int n = 0; n < count; n++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[n]);
            LocalSoundEffect effect = AssetDatabase.LoadAssetAtPath<LocalSoundEffect>(path);
            effects.Add(effect.name, effect);
        }
    }
#endif


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
                    if (effects.ContainsKey(soundName)) 
                    {
                        int distance = effects[soundName].distance;
                        writer.WriteVector3Packed(location);
                        writer.WriteStringPacked(soundName);
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
            if(effects.ContainsKey(clipName))
            {
                LocalSoundEffect soundEffect = effects[clipName];
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
                    source.PlayOneShot(soundEffect.audioClips[random], soundEffect.volume);
                    StartCoroutine(WaitForFinish(source, soundEffect.audioClips[random].length));
                }
                else // Only 1 Sound Effect
                {
                    source.PlayOneShot(soundEffect.audioClips[0], soundEffect.volume);
                    StartCoroutine(WaitForFinish(source, soundEffect.audioClips[0].length));
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
