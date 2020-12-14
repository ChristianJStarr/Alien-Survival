using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleObject : MonoBehaviour
{
    public int particleId = 0;
    public ParticleSystem particleSystem;
    
    public void Activate() 
    {
        particleSystem.Play();
    }
}
