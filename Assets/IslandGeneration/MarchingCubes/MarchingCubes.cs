using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes
{


    public MeshData MarchCubes(int[,,] noiseGrid)
    {

        int gridX = noiseGrid.GetLength(0);
        int gridY = noiseGrid.GetLength(1);
        int gridZ = noiseGrid.GetLength(2);

        MeshData meshData = new MeshData(gridX, gridY, gridZ);
        for (int x = 0; x < gridX - 1; x++)
        {
            for (int y = 0; y < gridY - 1; y++)
            {
                for (int z = 0; z < gridZ - 1; z++)
                {

                    int cubeIndex = 0;
                    int[] vertValues = {
                        noiseGrid[x, y, z],
                        noiseGrid[x, y, z + 1],
                        noiseGrid[x + 1, y, z + 1],
                        noiseGrid[x + 1, y ,z],
                        noiseGrid[x, y + 1, z],
                        noiseGrid[x, y + 1, z + 1],
                        noiseGrid[x + 1, y + 1, z + 1],
                        noiseGrid[x + 1, y + 1, z],
                    };

                    for (int i = 0; i < 8; i++)
                    {
                        if (vertValues[i] == 1)
                        {
                            cubeIndex |= 1 << i;
                        }
                    }

                    int[] triangulation = TriangleTable.triTable[cubeIndex];
                    //Debug.Log(string.Join(",", triangulation));
                    for (int i = 0; i < 12; i += 3)
                    {
                        if (triangulation[i] != -1)
                        {
                            Vector3 offset = new Vector3(x - gridX / 2, y - gridY / 2, z - gridZ / 2);

                            Vector3 vertex1 = TriangleTable.midPointFromIndex(triangulation[i]);
                            Vector3 vertex2 = TriangleTable.midPointFromIndex(triangulation[i + 1]);
                            Vector3 vertex3 = TriangleTable.midPointFromIndex(triangulation[i + 2]);

                            vertex1 += offset;
                            vertex2 += offset;
                            vertex3 += offset;

                            meshData.AddVertices(vertex1, vertex2, vertex3);
                            meshData.AddTriangle();

                        }
                    }
                }
            }
        }
        return meshData;
    }


    /*
    public MeshData MarchCubes(int[,,] noiseGrid)
    {

        int gridX = noiseGrid.GetLength(0);
        int gridY = noiseGrid.GetLength(1);
        int gridZ = noiseGrid.GetLength(2);

        int[,,] usedMidpoints = new int[(2 * gridX) - 1, (2 * gridY) - 1, (2 * gridZ) - 1];

        MeshData meshData = new MeshData(gridX, gridY, gridZ);
        for (int x = 0; x < gridX - 1; x++)
        {
            for (int y = 0; y < gridY - 1; y++)
            {
                for (int z = 0; z < gridZ - 1; z++)
                {

                    int cubeIndex = 0;
                    int[] vertValues = {
                        noiseGrid[x, y, z],
                        noiseGrid[x, y, z + 1],
                        noiseGrid[x + 1, y, z + 1],
                        noiseGrid[x + 1, y ,z],
                        noiseGrid[x, y + 1, z],
                        noiseGrid[x, y + 1, z + 1],
                        noiseGrid[x + 1, y + 1, z + 1],
                        noiseGrid[x + 1, y + 1, z],
                    };

                    for (int i = 0; i < 8; i++)
                    {
                        if (vertValues[i] == 1)
                        {
                            cubeIndex |= 1 << i;
                        }
                    }

                    int[] triangulation = TriangleTable.triTable[cubeIndex];
                    Vector3 offset = new Vector3(x - gridX / 2, y - gridY / 2, z - gridZ / 2);
                    int[] currentpoint = { x, y, z };

                    for (int i = 0; i < 12; i += 3)
                    {
                        if (triangulation[i] != -1)
                        {
                            Vector3[] triVerts = new Vector3[3];
                            int[] triInd = new int[3];
                            for (int j = 0; j < 3; j++)
                            {
                                int[] point = TriangleTable.getMidPointIndex(triangulation[j + i], currentpoint);
                                //int index = meshData.triangles[point[0] * gridX * gridY + point[1] * gridX + point[2]];
                                int index = usedMidpoints[point[0], point[1], point[2]];
                                if (index != 0)
                                {
                                    triVerts[j] = meshData.vertices[index];
                                    triInd[j] = index;
                                    //meshData.vertexIndex++;
                                }
                                else
                                {
                                    usedMidpoints[point[0], point[1], point[2]] = meshData.vertexIndex;
                                    triVerts[j] = TriangleTable.midPointFromIndex(triangulation[j + i]);
                                    triInd[j] = meshData.vertexIndex + j;
                                    //meshData.vertexIndex++;
                                }
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                triVerts[j] += offset;
                            }
                            

                            meshData.AddVertices(triVerts[0], triVerts[1], triVerts[2]);
                            meshData.AddTriangle(triInd[0],triInd[1],triInd[2]);

                        }
                    }
                }
            }
        }

        /*
        for (int x = 0; x < usedMidpoints.GetLength(0); x++)
        {
            for (int y = 0; y < usedMidpoints.GetLength(0); y++)
            {
                for (int z = 0; z < usedMidpoints.GetLength(0); z++)
                {
                    Debug.Log(usedMidpoints[x, y, z]);
                }
            }
        }
        
                    return meshData;
    }
}
*/
}
    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        int triangleIndex = 0;
        public int vertexIndex = 0;
        int UVIndex = 0;
        public MeshData(int meshWidth, int meshHeight, int meshLength)
        {
            uvs = new Vector2[meshWidth * meshHeight * meshLength * 5];
            vertices = new Vector3[meshWidth * meshHeight * meshLength * 5];
            triangles = new int[meshWidth * meshHeight * meshLength * 5];
        }
        public void AddTriangle()
        {
            triangles[triangleIndex] = triangleIndex;
            triangles[triangleIndex + 1] = triangleIndex + 1;
            triangles[triangleIndex + 2] = triangleIndex + 2;
            triangleIndex += 3;
        }
        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
        public void AddVertices(Vector3 a, Vector3 b, Vector3 c)
        {
            vertices[vertexIndex] = a;
            AddUVPosition(a);
            vertices[vertexIndex + 1] = b;
            AddUVPosition(b);
            vertices[vertexIndex + 2] = c;
            AddUVPosition(c);
            vertexIndex += 3;
        }
        public void AddUVPosition(Vector3 a)
        {
            uvs[UVIndex] = new Vector2(a.x, a.y);
            UVIndex++;
        //Debug.Log("UV ADDED AT: " + uvs[UVIndex - 1]);
        }
        public Mesh CreateMesh()
        {

            Mesh mesh = new Mesh();
            System.Array.Resize<Vector3>(ref vertices, vertexIndex);
            System.Array.Resize<int>(ref triangles, vertexIndex);
            System.Array.Resize<Vector2>(ref uvs, UVIndex);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
