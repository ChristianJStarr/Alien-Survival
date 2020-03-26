using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool colorMode;
    public const int mapChunkSize = 500;
    public NoiseData noiseData;

    public Noise.NormalizeMode normalizeMode;

    public Slider m_noiseScale;
    public Slider m_octaves;
    public Slider m_persistance;
    public Slider m_lacunarity;
    public Slider m_offset;
    public TextMeshProUGUI t_noiseScale;
    public TextMeshProUGUI t_octaves;
    public TextMeshProUGUI t_persistance;
    public TextMeshProUGUI t_lacunarity;
    public TextMeshProUGUI t_offset;

    public TerrainType[] regions;

    private bool generating;
    private bool isGenQueue;

    float[,] falloffMap;


    void Awake() 
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

   

    void Start() 
    {

        GenerateMap();
        t_noiseScale.text = (Mathf.Round(m_noiseScale.value * 100f) / 100f) + "";
        t_octaves.text = m_octaves.value + "";
        t_persistance.text = (Mathf.Round(m_persistance.value * 100f) / 100f) + "";
        t_lacunarity.text = m_lacunarity.value + "";
        t_offset.text = m_offset.value + "";
    }

    public void SetNoiseScale()
    {
        t_noiseScale.text = (Mathf.Round(m_noiseScale.value * 100f) / 100f) + "";
        noiseScale = m_noiseScale.value;
        QueueGenerateMap();
    }
    public void SetOctaves()
    {
        t_octaves.text = m_octaves.value + "";
        octaves = (int) m_octaves.value;
        QueueGenerateMap();
    }
    public void SetPersistance()
    {
        t_persistance.text = (Mathf.Round(m_persistance.value * 100f) / 100f) + "";
        persistance = m_persistance.value;
        QueueGenerateMap();
    }
    public void SetLacunarity()
    {
        t_lacunarity.text = m_lacunarity.value + "";
        lacunarity = m_lacunarity.value;
        QueueGenerateMap();
    }
    public void SetOffset()
    {
        t_offset.text = m_offset.value + "";
        seed = (int)m_offset.value;
        QueueGenerateMap();
    }


    private void QueueGenerateMap() 
    {
        if (!generating) 
        {
            generating = true;
            GenerateMap();
            StartCoroutine(GenerateWait());
        }
        else 
        {
            isGenQueue = true;
        }
    }

    IEnumerator GenerateWait() 
    {
        yield return new WaitForSeconds(2f);
        generating = false;
        if(isGenQueue == true) 
        {
            QueueGenerateMap();
            isGenQueue = false;
        }
    }

    public void GenerateMap() 
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset, normalizeMode) ;

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = noiseMap[x, y];
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (colorMode)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        else 
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
       
       

    }


    public void SaveMap() 
    {
        noiseData.octaves = octaves;
        noiseData.noiseScale = noiseScale;
        noiseData.persistance = persistance;
        noiseData.lacunarity = lacunarity;
        noiseData.seed = seed;
    }
    [System.Serializable]
    public struct TerrainType 
    {
        public string name;
        public float height;
        public Color color;
    }

}
