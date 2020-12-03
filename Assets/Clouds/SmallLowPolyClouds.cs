using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class SmallLowPolyClouds : MonoBehaviour
{
    public Mesh cloudMesh;
    public Material cloudMat;

    public float threshold;
    public Vector2 noiseOffset;
    public float noiseScale;
    public float scaleSpeed;
    public float maxScale;

    public float cloudMeshScale;

    private const int gridSize = 30;
    private int midpoint = gridSize / 2;
    private const float maxGridDistance = 21.213203435596426f; //distance from origin to (15,15)

    private Cloud[] clouds;

    //How long it takes to make a full rotation
    public float rotationFrequency;

    private class Cloud
    {
        public float scale;
        public int x; //these refer to the position on the noise grid, not game space
        public int z;
        public Matrix4x4 matrix;
    }

    private void Start()
    {

        int maxNumberOfClouds = gridSize * gridSize;
        clouds = new Cloud[maxNumberOfClouds];

        int cloudsAdded = 0;

        for (int gridX = 0; gridX < gridSize; gridX++)
        {
            for (int gridZ = 0; gridZ < gridSize; gridZ++)
            {
                float dist = Mathf.InverseLerp(0, maxGridDistance, distance(gridX - midpoint, gridZ - midpoint));

                if (dist > 0.2 && dist < 0.7) {

                    Vector3 position = new Vector3
                    {
                        x = transform.position.x + ((gridX - midpoint) * cloudMeshScale),
                        y = transform.position.y,
                        z = transform.position.z + ((gridZ - midpoint) * cloudMeshScale)
                    };

                    clouds[cloudsAdded] = new Cloud
                    {
                        x = gridX,
                        z = gridZ,
                        scale = 0,
                        matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one),
                    };
                    cloudsAdded++;
                }
            }
        }
        System.Array.Resize(ref clouds, cloudsAdded);
    }



    private void Update()
    {
        float angle = (Time.time * Mathf.PI * 2 / rotationFrequency) % (Mathf.PI * 2);
        //ParallelOptions options = new ParallelOptions();

        Matrix4x4[] objects = new Matrix4x4[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            float x = clouds[i].x - midpoint;
            float z = clouds[i].z - midpoint;

            //rotate our original locations by the angle based on the time
            float[] samples = rotateVector(x, z, angle);
            float noise = (Mathf.PerlinNoise((samples[0] / noiseScale) + noiseOffset.x, (samples[1] / noiseScale) + noiseOffset.y) + 1) / 2;

            //with this noise value we can decide if it should be scaling down or up
            //based on the threshold value
            int scaleDir = noise > threshold ? 1 : -1;

            float scaleDelta = scaleSpeed * Time.deltaTime * scaleDir;
            float newScale = clouds[i].scale + scaleDelta;
            newScale = Mathf.Clamp(newScale, 0, maxScale);

            //and now to apply it to the cloud and object
            clouds[i].scale = newScale;
            clouds[i].matrix.m00 = newScale;
            clouds[i].matrix.m11 = newScale;
            clouds[i].matrix.m22 = newScale;
            objects[i] = clouds[i].matrix;
        }

        Graphics.DrawMeshInstanced(cloudMesh, 0, cloudMat, objects);
    }


    private static float[] rotateVector(float x, float y, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float tx = x;
        float ty = y;

        return new float[] {cos * tx - sin * ty,
                            sin * tx + cos * ty};
    }

    private static float distance(float x, float y)
    {
        return Mathf.Sqrt(x * x + y * y);
    }
}
