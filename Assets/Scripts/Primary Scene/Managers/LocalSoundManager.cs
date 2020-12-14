using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalSoundManager : MonoBehaviour
{
    #region Singleton 
    public static LocalSoundManager Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    [SerializeField] LocalSoundEffect[] soundEffects;
    private Queue<AudioSource> pool = new Queue<AudioSource>();
    public int maxPoolSize = 20;
    private int currentUsed = 0;

    #region Gather Scriptable Objects OnValidate Serialize
#if UNITY_EDITOR
    private bool serializeScriptables = true;
    private void OnValidate()
    {
        if (serializeScriptables) 
        {
            string[] guids = AssetDatabase.FindAssets("t:LocalSoundEffect", new[] { "Assets/Content/LocalSoundEffects" });
            int count = guids.Length;
            soundEffects = new LocalSoundEffect[count];
            for (int n = 0; n < count; n++)
            {
                soundEffects[n] = AssetDatabase.LoadAssetAtPath<LocalSoundEffect>(AssetDatabase.GUIDToAssetPath(guids[n]));
            }
        }
    }
#endif
    #endregion


    //Create Audio Source Instance
    private AudioSource CreateNewInstance()
    {
        GameObject go = new GameObject("AudioSourceInstance");
        go.transform.parent = transform;
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

    //Called from Server
    public void PlaySoundEffect(int soundEffectId, Vector3 position)
    {
        LocalSoundEffect soundEffect = GetSoundEffectById(soundEffectId);
        AudioSource source = GetAudioSource();
        if(soundEffect != null && source != null) 
        {
            source.transform.position = position;
            source.maxDistance = soundEffect.distance;
            int index = 0;
            if (soundEffect.audioClips.Length > 1)
            {
                index = Random.Range(0, soundEffect.audioClips.Length - 1);
                if (soundEffect.lastPlayedIndex != 99 && soundEffect.lastPlayedIndex == index)
                {
                    index = Random.Range(0, soundEffect.audioClips.Length - 1);
                    if (soundEffect.lastPlayedIndex == index)
                    {
                        index = Random.Range(0, soundEffect.audioClips.Length - 1);
                    }
                }
                soundEffect.lastPlayedIndex = index;
            }
            source.PlayOneShot(soundEffect.audioClips[index], soundEffect.volume);
            StartCoroutine(WaitForFinish(source, soundEffect.audioClips[index].length));
        }
    }

    //Get Local Sound Effect from ID
    private LocalSoundEffect GetSoundEffectById(int soundEffectId) 
    {
        int length = soundEffects.Length;
        for (int i = 0; i < length; i++)
        {
            if(soundEffects[i].soundEffectId == soundEffectId) 
            {
                return soundEffects[i];
            }
        }
        return null;
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
        if(currentUsed < maxPoolSize) 
        {
            pool.Enqueue(instance);
        }
        else 
        {
            currentUsed--;
            Destroy(instance.gameObject);
        }
    }

}
