using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Master Music Manager Script
/// </summary>
public class MusicManager : MonoBehaviour
{
    /// <summary>
    /// Audio Source for Music
    /// </summary>
    public AudioSource musicAudio;
    /// <summary>
    /// Audio Source for Ambient Sounds
    /// </summary>
    public AudioSource ambientAudio;
    /// <summary>
    /// Audio Source for UI Clicks
    /// </summary>
    public AudioSource uiAudio;
    /// <summary>
    /// Reference to Game Settings
    /// </summary>
    public Settings settings;
    /// <summary>
    /// Game Music Clip. Primary Scene
    /// </summary>
    public AudioClip gameClip;
    /// <summary>
    /// Main Menu Music Clip. MainMenu Scene
    /// </summary>
    public AudioClip menuClip;
    /// <summary>
    /// Ambient Sounds Clip. Primary Scene
    /// </summary>
    public AudioClip ambientClip;
    private float musicInc;
    private float ambientInc;

    /// <summary>
    /// Music Manager Start Function: Change() and remove if duplicate.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void Start()
    {
        DontDestroyOnLoad(this);
        Change();
        if(FindObjectsOfType<MusicManager>().Length > 1) 
        {
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// Change sound volume to stored settings.
    /// </summary>
    public void Change() 
    {
        musicAudio.volume = settings.musicVolume;
        ambientAudio.volume = settings.ambientVolume;
        musicInc = settings.musicVolume / 20;
        ambientInc = settings.ambientVolume / 20;
        uiAudio.volume = settings.uiVolume;
    }
    /// <summary>
    /// OnLevelWasLoaded for handling which audio plays on what scene.
    /// </summary>
    /// <param name="level"></param>
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
    /// <summary>
    /// Play Button Click Sound Effect.
    /// </summary>
    public void ButtonClick() 
    {
        uiAudio.Play();
    }
    /// <summary>
    /// Fade sound clips. Used for scene switching.
    /// </summary>
    /// <param name="audioClip">New Audio Clip for Music</param>
    /// <param name="audioClip2">New Audio Clip for Ambient</param>
    /// <returns></returns>
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

