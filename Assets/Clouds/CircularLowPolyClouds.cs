using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class CircularLowPolyClouds : MonoBehaviour
{
    public Mesh cloudMesh;
    public Material cloudMat;

    public float threshold;
    public Vector2 noiseOffset;
    public float noiseScale;
    public float scaleSpeed;
    public float maxScale;

    public float cloudMeshScale;

    public int gridSize;
    private float maxGridDistance;
    private const int BATCHSIZE = 1023;
    private Batch[] batches;

    //How long it takes to make a full rotation
    public float rotationFrequency;

    private struct Cloud
    {
        public float scale;
        public int x; //these refer to the position on the noise grid, not game space
        public int z;
    }
    
    private struct Batch
    {
        public int length;
        public Cloud[] clouds;
        public Matrix4x4[] objects;
    }
   
    private struct TimeInformation
    {
        public float time;
        public float deltaTime;
        public int batchIndex;
        public float angle;
    }
    private void Start()
    {

        int numberOfClouds = gridSize * gridSize;
        maxGridDistance = distance(gridSize,gridSize);


        //number of batches
        //batches is a group of clouds with a max of 1024
        //this is because unity gpu instancing can only hold 1024 elements

        int numberOfBatches = numberOfClouds / BATCHSIZE + 1;
        batches = new Batch[numberOfBatches];
        for (int i = 0; i < numberOfBatches; i++)
        {
            int length = Mathf.Min(BATCHSIZE, numberOfClouds - i * BATCHSIZE);
            batches[0] = new Batch
            {
                length = length,
                clouds = new Cloud[length],
                objects = new Matrix4x4[length]
            };
        }
            
        
        int cloudsAdded = 0;
        int batchesAdded = 0;
        int midpoint = gridSize / 2;
        for (int cloudX = 0; cloudX < gridSize; cloudX++)
        {
            for (int cloudZ = 0; cloudZ < gridSize; cloudZ++)
            {
                //initialize clouds
                batches[batchesAdded].clouds[cloudsAdded] = new Cloud
                {
                    x = cloudX,
                    z = cloudZ,
                    scale = 0,
                };
                Vector3 position = new Vector3
                {
                    x = transform.position.x + ((cloudX - midpoint) * cloudMeshScale),
                    y = transform.position.y,
                    z = transform.position.z + ((cloudZ - midpoint) * cloudMeshScale)
                };

                batches[batchesAdded].objects[cloudsAdded] = Matrix4x4.TRS(position,
                    Random.rotation,
                    Vector3.zero);

                //increment cloud and batch indices
                if (cloudsAdded < BATCHSIZE)
                {
                    cloudsAdded++;
                }
                else
                {
                    cloudsAdded = 0;
                    batches[batchesAdded].length = BATCHSIZE;
                    batchesAdded++;
                }
            }
        }
        batches[batchesAdded].length = cloudsAdded;
        
    }


    private void Update()
    { 
        //Essentially the threads
        Task[] tasks = new Task[batches.Length];
        for (int batch = 0; batch < batches.Length; batch++)
        {
            TimeInformation timeInfo = new TimeInformation
            {
                time = Time.time,
                deltaTime = Time.deltaTime,
                batchIndex = batch,
                //Represents the angle to rotate to based on the time elapsed
                angle = (Time.time * Mathf.PI * 2 / rotationFrequency) % (Mathf.PI * 2)

            };
            tasks[batch] = Task.Factory.StartNew(UpdateBatch, timeInfo);

        }

        Task.WaitAll(tasks);

        for (int batch = 0; batch < batches.Length; batch++)
        {
            Graphics.DrawMeshInstanced(cloudMesh, 0, cloudMat, batches[batch].objects);
        }
    }
    

    private void UpdateBatch(object input)
    {
        TimeInformation tInfo = (TimeInformation)input;
        int midpoint = gridSize / 2;
        for (int c = 0; c < batches[tInfo.batchIndex].length; c++)
        {
            int i = tInfo.batchIndex;

            //calculating noise//
            //this just centers the array so the center is (0,0)
            float x = batches[i].clouds[c].x - midpoint;
            float z = batches[i].clouds[c].z - midpoint;

            int scaleDir;
            float newScale;
            float dist = Mathf.InverseLerp(0, maxGridDistance, distance(x, z));

            //If it is not in the "cloud ring" just set it to 0
            //And skip calculating noise
            if (dist < 0.2 || dist > 0.3)
            {
                newScale = 0;
            }
            else
            {
                //rotate our original locations by the angle based on the time
                float[] samples = rotateVector(x, z, tInfo.angle);
                float noise = (Mathf.PerlinNoise((samples[0] / noiseScale) + noiseOffset.x, (samples[1] / noiseScale) + noiseOffset.y) + 1) / 2;

                //with this noise value we can decide if it should be scaling down or up
                //based on the threshold value
                scaleDir = noise > threshold ? 1 : -1;
                //Now to calculate the scaling
                float scaleDelta = scaleSpeed * tInfo.deltaTime * scaleDir;
                newScale = batches[i].clouds[c].scale + scaleDelta;
                newScale = Mathf.Clamp(newScale, 0, maxScale);
            }
            
            
            
            
            //and now to apply it to the cloud and object
            batches[i].clouds[c].scale = newScale;
            batches[i].objects[c].m00 = newScale;
            batches[i].objects[c].m11 = newScale;
            batches[i].objects[c].m22 = newScale;

        }
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
