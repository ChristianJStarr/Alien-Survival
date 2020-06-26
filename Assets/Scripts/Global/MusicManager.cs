using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicAudio;
    public AudioSource ambientAudio;
    public AudioSource uiAudio;
    public Settings settings;
    public AudioClip gameClip;
    public AudioClip menuClip;
    public AudioClip ambientClip;
    private float musicInc;
    private float ambientInc;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
#if UNITY_SERVER
        musicAudio.Stop();
        ambientAudio.Stop();
        uiAudio.Stop();
        return;
#endif
        musicAudio.Stop();
        ambientAudio.Stop();
        uiAudio.Stop();
        return;

        DontDestroyOnLoad(this);
        Change();
        if(FindObjectsOfType<MusicManager>().Length > 1) 
        {
            Destroy(this.gameObject);
        }
    }
   
    //Update Values to Settings
    public void Change() 
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
#if UNITY_SERVER
        return;
#endif

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
    public void ButtonClick() 
    {
        uiAudio.Play();
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
}

