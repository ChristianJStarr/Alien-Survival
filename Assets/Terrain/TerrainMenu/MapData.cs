using UnityEngine;

public static class MapData
{
    private static int mapWidth, mapHeight, octaves, seed;
    private static float noiseScale, persistance, lacunarity;

    public static int MapWidth { get { return mapWidth; } set { mapWidth = value; } }
    public static int MapHeight { get { return mapHeight; } set { mapHeight = value; } }
    public static int Octaves { get { return octaves; } set { octaves = value; } }
    public static int Seed { get { return seed; } set { seed = value; } }
    public static float NoiseScale { get { return noiseScale; } set { noiseScale = value; } }
    public static float Persistance { get { return persistance; } set { persistance = value; } }
    public static float Lacunarity { get { return lacunarity; } set { lacunarity = value; } }
}