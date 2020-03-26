using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class GameMapGenerator : MonoBehaviour
{
    public NoiseData noiseData;
    public TerrainData terrainData;
    public TextureData textureData;

    public const int mapChunkSize = 239;
    [Range(0, 6)]
    public int levelOfDetail;
    public Noise.NormalizeMode normalizeMode;
    public bool autoUpdate;
    public TerrainType[] regions;
    public Material terrainMaterial;
    float[,] fallOffMap;

    Queue<MapThreadInfo<MapDat>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapDat>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    void Awake() 
    {
        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void DrawMapInEditor()
    {
        MapDat mapData = GenerateMapData(Vector2.zero);

        TerrainDisplay display = FindObjectOfType<TerrainDisplay>();
        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colourMap, mapChunkSize, mapChunkSize));
    }

    void OnTextureValuesUpdated() 
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public void RequestMapData(Vector2 center, Action<MapDat> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapDat> callback)
    {
        MapDat mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapDat>(callback, mapData));
        }
    }

    public void RequestMeshData(MapDat mapData,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapDat mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapDat> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapDat GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize+2, mapChunkSize+2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (terrainData.useFalloff) 
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);

                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].colour;
                    }
                    else 
                    {
                        break;
                    }
                }
            }
        }


        return new MapDat(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if (noiseData.lacunarity < 1)
        {
            noiseData.lacunarity = 1;
        }
        if (noiseData.octaves < 0)
        {
            noiseData.octaves = 0;
        }
        //fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapDat
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapDat(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
