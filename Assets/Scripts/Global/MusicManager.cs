using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    #region Singleton
    public static MusicManager Singleton;
    private void Awake() { Singleton = this; }
    #endregion
    public Settings settings;
    public AudioSource musicAudio, ambientAudio, uiAudio;
    public AudioClip gameClip, menuClip, ambientClip;

    public AudioClip[] uiSounds;


    private float musicInc, ambientInc;
#if !UNITY_SERVER

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SettingsMenu.ChangedSettings += Change;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SettingsMenu.ChangedSettings -= Change;
    }


    void Start()
    {


        DontDestroyOnLoad(this.gameObject);
        Change();
        if(FindObjectsOfType<MusicManager>().Length > 1) 
        {
            Destroy(this.gameObject);
        }
    }
   
    //Update Values to Settings
    private void Change() 
    {
        musicAudio.volume = settings.musicVolume;
        ambientAudio.volume = settings.ambientVolume;
        musicInc = settings.musicVolume / 20;
        ambientInc = settings.ambientVolume / 20;
        uiAudio.volume = settings.uiVolume;
    }
    
    //On Scene Loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        int level = scene.buildIndex;
        if(level == 2) 
        {
            GameObject tmp = new GameObject("Temp Listener");
            tmp.AddComponent<AudioListener>();
            Destroy(tmp, 10);
            StartCoroutine(FadeSound(ambientClip, gameClip));
        }
        if(level == 1 && musicAudio.clip != menuClip) 
        {
            musicAudio.clip = menuClip;
            musicAudio.Play();
            ambientAudio.clip = null;
        }
        if (level == 0 && musicAudio.clip != menuClip)
        {
            musicAudio.clip = menuClip;
            musicAudio.Play();
            ambientAudio.clip = null;
        }
    }

    //Button Click, Player UI Sound
    public static void PlayUISound(int soundId) 
    {
        if(Singleton != null) 
        {
            Singleton.PlayUISoundTask(soundId);
        }
    }

    //Play UI Sound Task
    public void PlayUISoundTask(int soundId) 
    {
        if(uiSounds[soundId] != null) 
        {
            uiAudio.clip = uiSounds[soundId];
            uiAudio.Play();
        }
    }

    //Fade Sound Volume9
    private IEnumerator FadeSound(AudioClip audioClip, AudioClip audioClip2)
    {
        yield return new WaitForSeconds(0.2F);
        bool restart = false;
        if (ambientAudio.clip != audioClip)
        {
            if (ambientAudio.volume >= ambientInc)
            {
                ambientAudio.volume -= ambientInc;
                restart = true;
                
            }
            else if (ambientAudio.volume < ambientInc && ambientAudio.volume != 0)
            {
                ambientAudio.volume = 0;
                restart = true;
            }
            else if (ambientAudio.volume == 0)
            {
                ambientAudio.clip = audioClip;
                ambientAudio.Play();
                restart = true;
            }
        }
        else 
        {
            float volume = settings.ambientVolume;
            if (ambientAudio.volume < volume)
            {
                ambientAudio.volume += ambientInc;
                restart = true;
            }
            else if (ambientAudio.volume >= volume)
            {
                ambientAudio.volume = volume;
            }
        }
            if (musicAudio.clip != audioClip2)
            {
                if (musicAudio.volume >= musicInc)
                {
                    musicAudio.volume -= musicInc;
                    restart = true;
                }
                else if (musicAudio.volume < musicInc && musicAudio.volume != 0)
                {
                    musicAudio.volume = 0;
                    restart = true;
                }
                else if (musicAudio.volume == 0)
                {
                    musicAudio.clip = audioClip2;
                    musicAudio.Play();
                    restart = true;
                }
            }
            else
            {
                float volume = settings.musicVolume;
                if (musicAudio.volume < volume)
                {
                    musicAudio.volume += musicInc;
                    restart = true;
                }
                else if (musicAudio.volume >= volume)
                {
                    musicAudio.volume = volume;
                }
            }
        
        if (restart) 
        {
            StartCoroutine(FadeSound(audioClip, audioClip2));
        }
    }

#else
    private void Start()
    {        
        musicAudio.Stop();
        ambientAudio.Stop();
        uiAudio.Stop();
        Destroy(this);
        return;
    }
#endif
}

