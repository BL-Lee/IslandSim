using UnityEngine;

public static class NoiseGenerator3D
{
    

    public static float Perlin3D(float x, float y, float z, Vector3 offset, float scale)
    {
        float AB = Mathf.PerlinNoise(x * scale + offset.x, y * scale + offset.y);
        float BC = Mathf.PerlinNoise(y * scale + offset.y, z * scale + offset.z);
        float AC = Mathf.PerlinNoise(x * scale + offset.x, z * scale + offset.z);

        float BA = Mathf.PerlinNoise(y * scale + offset.y, x * scale + offset.x);
        float CB = Mathf.PerlinNoise(z * scale + offset.z, y * scale + offset.y);
        float CA = Mathf.PerlinNoise(z * scale + offset.z, x * scale + offset.x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6;
    }
    public static float BasePerlin3D(float x, float y, float z)
    {
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6;
    }
    public static int[,,] CutEdges(int[,,] inpMap)
    {
        int size = inpMap.GetLength(0);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (x == 0 || x == size - 1 ||
                        y == 0 || y == size - 1 ||
                        z == 0 || z == size - 1)
                    {
                        inpMap[x, y, z] = 1;
                    }
                }
            }
        }
        return inpMap;
    }
    public static int[,,] CreateNoiseGrid(int size, Vector3 offset, float threshold, float noiseScale)
    {
        float[,,] noiseGrid = new float[size,size,size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    noiseGrid[x, y, z] = Perlin3D((float)x, (float)y , (float)z , offset, noiseScale); 
                }
            }
        }

        return ClampNoiseGrid(threshold,noiseGrid);
    }
    public static int[,,] ClampNoiseGrid(float threshold,float[,,] noiseGrid)
    {
        int size = noiseGrid.GetLength(0);
        int[,,] clampedGrid = new int[size, size, size];
        for (int x = 0; x < noiseGrid.GetLength(0); x++)
        {
            for (int y = 0; y < noiseGrid.GetLength(1); y++)
            {
                for (int z = 0; z < noiseGrid.GetLength(2); z++)
                {
                    if (noiseGrid[x,y,z] > threshold) { clampedGrid[x, y, z] = 0; }
                    else { clampedGrid[x, y, z] = 1; }
                }
            }
        }
        return CutEdges(clampedGrid);

    }
    
    public static int[,,] IslandGrid(int size, AnimationCurve curveHeight, AnimationCurve plane)
    {
        int[,,] map = new int[size, size, size];
        System.Random r = new System.Random();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    float spot = curveHeight.Evaluate(y / (float)size) + plane.Evaluate(z / (float)size) + plane.Evaluate(x / (float)size);

                    spot = (spot / 3);
                    
                    if (spot > 0.5) { map[x, y, z] = 0; }
                    else {
                        map[x, y, z] = 1;
                        
                    }

                    if (y == size-1 || x == size-1 || z == size-1) { map[x, y, z] = 1; }
                    if (y == 0 || x == 0 || z == 0) { map[x, y, z] = 1; }

                    
                }
            }
        }
        return map;
    }
    
    
    public static float[,,] ClassicGrid(int size, Vector3 offset, float scale,
        int octaves, float lacunarity, float persistance, float sphereFactor, float yFactor)
    {
        OpenSimplexNoise noiseClass = new OpenSimplexNoise(0);
        if (scale <= 0) { scale = 0.0001f; }
        float minNoise = float.MaxValue;
        float maxNoise = float.MinValue;
        float[,,] noiseMap = new float[size, size, size];
        float center = (float)size / 2;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x / scale * frequency) + offset.x;
                        float sampleY = (y/ scale * frequency) + offset.y;
                        float sampleZ = (z/ scale * frequency) + offset.z;
                        float simplexValue = (float)noiseClass.Evaluate(sampleX, sampleY, sampleZ);
                        
                        noiseHeight += simplexValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;

                    }
                    
                    noiseHeight -=  Mathf.Pow(
                        (x - center) * (x - center) +
                        (y - center) * (y - center) +
                        (z - center) * (z - center)
                        , 0.5f)/sphereFactor;

                    if (y/(float)size < 0.7)
                    {
                        noiseHeight *= 1-((y / (float)size)-0.4f) * yFactor;
                    }

                    //noiseHeight *= (float)noiseClass.Evaluate(x / scale, y / scale, z / scale) + 2;
                    
                    if (noiseHeight < minNoise)
                    {
                        minNoise = noiseHeight;
                    }
                    if (noiseHeight > maxNoise)
                    {
                        maxNoise = noiseHeight;
                    }
                
                    noiseMap[x, y, z] = noiseHeight;
                    
                }

            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    noiseMap[x, y, z] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y, z]);
                    
                }
            }
        }
        return noiseMap;
    }
    
    public static float[,,] CircleGrid(int size,  float scale)
    {
        
        if (scale <= 0) { scale = 0.0001f; }
        float minNoise = float.MaxValue;
        float maxNoise = float.MinValue;
        float[,,] noiseMap = new float[size, size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {

                    float center = (float)size / 2;
                    
                    float noiseHeight = Mathf.Pow(
                        (x - center) * (x - center) +
                        (y - center) * (y - center) +
                        (z - center) * (z - center)
                        ,0.5f);

                    if (noiseHeight < minNoise)
                    {
                        minNoise = noiseHeight;
                    }
                    if (noiseHeight > maxNoise)
                    {
                        maxNoise = noiseHeight;
                    }
                    
                    noiseMap[x, y, z] = noiseHeight;

                }
            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    noiseMap[x, y, z] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y, z]);
                    noiseMap[x, y, z] = 1 - noiseMap[x, y, z];
                }
            }
        }
        return noiseMap;
    }
}
