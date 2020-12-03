using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IslandGenerator : MonoBehaviour
{
    
    public int size;
    public Vector3 offset;
    public float noiseScale;
    [Range(0,1)]
    public float threshold;
    public bool autoUpdate;

    public int octaves;
    
    public float lacunarity;
    [Range(0,1)]
    public float persistance;

    public float sphereWeight;
    public float yFactor;

    public ColourData colourData;
    public Material islandMaterial;
    
    MarchingCubes marchingCubes = new MarchingCubes();
    
    public void OnValidate()
    {
        if (autoUpdate)
        {
            
            IslandDisplay display = FindObjectOfType<IslandDisplay>();

            float[,,] simpMap = NoiseGenerator3D.ClassicGrid(size, offset, noiseScale, octaves, lacunarity, persistance, sphereWeight,yFactor);
            int[,,] noiseMap = NoiseGenerator3D.ClampNoiseGrid(threshold, simpMap);
            MeshData meshData = marchingCubes.MarchCubes(noiseMap);

            display.drawMesh(meshData,colourData, islandMaterial);
        }
    }
    

}
