using UnityEngine;

[CreateAssetMenu(fileName = "SFX_01", menuName = "ScriptableObjects/SoundEffectData")]
public class LocalSoundEffect : ScriptableObject
{
    public int distance = 500;
    public float volume = 1;
    public AudioClip[] audioClips;
    public int lastPlayedIndex = 99;
}
