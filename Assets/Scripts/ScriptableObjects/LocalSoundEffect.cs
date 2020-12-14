using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SFX_01", menuName = "ScriptableObjects/SoundEffectData")]
[Serializable]
public class LocalSoundEffect : ScriptableObject
{
    public int soundEffectId = 0;
    public int distance = 500;
    public float volume = 1;
    public AudioClip[] audioClips;
    public int lastPlayedIndex = 99;
}
