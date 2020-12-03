using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandDisplay : MonoBehaviour
{
    public Renderer renderer;
    public MeshFilter meshFilter;
    public MeshRenderer MeshRenderer;
    public MeshCollider meshCollider;
    ColourData colourData;



    public void drawMesh(MeshData meshData,ColourData colourData, Material material)
    {
//        Debug.Log(meshData);
        this.colourData = colourData;
        
        
        
        Mesh mesh = meshData.CreateMesh();
        
        float[] heights = findMinMax(mesh.uv);
        colourData.mat.SetFloat("minHeight", -15);
        colourData.mat.SetFloat("maxHeight", 15);
        //Debug.Log("Max height : " + heights[1]);
        //Debug.Log("Min height : " + heights[0]);
        //Debug.Log("height: " + mesh.uv[mesh.uv.Length / 2]);
        colourData.mat.SetColorArray("baseColours", colourData.baseColours);
        colourData.mat.SetFloatArray("baseStartHeights", colourData.baseStartHeights);
        colourData.mat.SetInt("baseColourCount", colourData.baseColours.Length);
        colourData.mat.SetFloatArray("baseBlends", colourData.baseBlends);

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    public static float[] findMinMax(Vector2[] vertices)
    {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < minHeight) { minHeight = vertices[i].y; }
            if (vertices[i].y > maxHeight) { maxHeight = vertices[i].y; }

        }
        return new float[] { minHeight, maxHeight };
    }
    

}
