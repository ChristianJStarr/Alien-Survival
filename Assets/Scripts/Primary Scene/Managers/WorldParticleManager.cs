using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldParticleManager : MonoBehaviour
{
    public static WorldParticleManager Singleton;
    public Dictionary<int, Queue<ParticleObject>> pool = new Dictionary<int, Queue<ParticleObject>>();
    public GameObject[] particlePrefabs;

    //Spawn a Particle by Id to Vector3
    public void SpawnParticle(int particleId, Vector3 position)
    {
        ParticleObject particle = GetFromPool(particleId);
        if(particle != null) 
        {
            particle.transform.position = position;
            particle.gameObject.SetActive(true);
            particle.Activate();
            StartCoroutine(WaitForFinish(particle));
        }
    }
    public void SpawnParticle(int particleId, Vector3 position, Quaternion quaternion)
    {
        ParticleObject particle = GetFromPool(particleId);
        if (particle != null)
        {
            particle.transform.position = position;
            particle.transform.rotation = quaternion;
            particle.gameObject.SetActive(true);
            particle.Activate();
            StartCoroutine(WaitForFinish(particle));
        }
    }
    
    
    //Wait for Particle to Finish before returning it to Pool
    private IEnumerator WaitForFinish(ParticleObject particle) 
    {
        yield return new WaitForSeconds(particle.particleSystem.main.duration);
        particle.gameObject.SetActive(false);
        if (pool.ContainsKey(particle.particleId))
        {
            pool[particle.particleId].Enqueue(particle);
        }
        else 
        {
            pool.Add(particle.particleId, new Queue<ParticleObject>());
            pool[particle.particleId].Enqueue(particle);
        }
    }
    
    //Get Particle Object from Particle Pool
    private ParticleObject GetFromPool(int particleId) 
    {
        ParticleObject instance = null;
        if (pool.ContainsKey(particleId))
        {
            instance = pool[particleId].Dequeue();
        }
        else
        {
            if (particlePrefabs[particleId] != null)
            {
                GameObject temp = Instantiate(particlePrefabs[particleId], Vector3.zero, Quaternion.identity, transform);
                temp.SetActive(false);
                instance = temp.GetComponent<ParticleObject>();
            }
        }
        return instance;
    }

}
