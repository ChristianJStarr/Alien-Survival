using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Benchmarker : MonoBehaviour
{
    public GameObject alienPrefab;
    public BenchmarkObject benchmark;

    private GameObject alienCurrent;


    private void Start()
    {
        if(NetworkingManager.Singleton == null && benchmark.isBenching) 
        {
            alienCurrent = Instantiate(alienPrefab, transform.position, transform.rotation);

        }        
    }
}
