using System.Threading.Tasks;
using UnityEngine;


/**
 * Taken from
 * URL: https://gist.github.com/imurashka/a48cd3743c8e55bdc9fcac24b021e3a5
 */
public class LowPolyClouds : MonoBehaviour
{
    [SerializeField]
    private Mesh m_mesh;
    [SerializeField]
    private Material m_material;
    [SerializeField]
    private float m_cloudSize;
    [SerializeField]
    private float m_maxScale = 1;
    [SerializeField]
    private float m_timeScale = 0.05f;
    [SerializeField]
    private float m_texScale = 0.1f;
    [SerializeField]
    private float m_minNoiseSize = 0.6f;
    [SerializeField]
    private float m_scaleSize = 1.5f;
    [SerializeField]
    private int m_cloudsCount = 100;

    //maximum size of matrices array that can be passed to Graphics.DrawMeshInstanced
    private const int BATCHSIZE = 1023;

    private Batch[] m_batches;
    private Task[] m_tasks;

    private struct Cloud
    {
        public float scale;
        public int x;
        public int y;
    }

    private struct Batch
    {
        public int length;
        public Cloud[] clouds;
        public Matrix4x4[] objects;
    }

    private struct FrameParams
    {
        public float time;
        public float deltaTime;
        public int batchIndex;
    }

    private void Start()
    {
        //total number of clouds
        int count = m_cloudsCount * m_cloudsCount;

        //total amount of batches
        int batchCount = count / BATCHSIZE + 1;

        m_batches = new Batch[batchCount];
        m_tasks = new Task[batchCount];
        //initializes the batches
        for (int i = 0; i < batchCount; i++)
        {

            int length = Mathf.Min(BATCHSIZE, count - i * BATCHSIZE); 
            m_batches[i].length = length;
            m_batches[i].clouds = new Cloud[length];
            m_batches[i].objects = new Matrix4x4[length];

        }

        //pivot of clouds should be at center so just shift each cloud

        float offset = -m_cloudsCount * 0.5f;
        //initialize data for each cloud
        for (int cloudY = 0; cloudY < m_cloudsCount; cloudY++)
        {
            for (int cloudX = 0; cloudX < m_cloudsCount; cloudX++)
            {
                Cloud cloud = new Cloud
                {
                    scale = 0,
                    x = cloudX,
                    y = cloudY
                };

                Vector3 position = new Vector3
                {
                    x = offset + transform.position.x + cloudX * m_cloudSize,
                    y = transform.position.y,
                    z = offset + transform.position.z + cloudY * m_cloudSize
                };

                int index = cloudY * m_cloudsCount + cloudX;

                int x = index / BATCHSIZE;
                int y = index % BATCHSIZE;
                
                m_batches[x].clouds[y] = cloud;
                m_batches[x].objects[y] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.zero);
            }
        }
    }
    private void Update()
    {


        //Each batch will be updated in a separate thread
        for (int batchIndex = 0; batchIndex < m_batches.Length; batchIndex++)
        {
            //to avoid allocations while creating delegates like ()=> UpdateBatch(frameParams)
            //im sending frame params as the object then cast to frameParams
            FrameParams frameParams = new FrameParams
            {
                batchIndex = batchIndex,
                deltaTime = Time.deltaTime,
                time = Time.time
            };

            //creation of new task allocates some memory but i cant do anything with it
            m_tasks[batchIndex] = Task.Factory.StartNew(UpdateBatch, frameParams);
        }
        Task.WaitAll(m_tasks);
        for (int batchIndex = 0; batchIndex < m_batches.Length; batchIndex++)
        {
            Graphics.DrawMeshInstanced(m_mesh, 0, m_material, m_batches[batchIndex].objects);
        }
    

    }

    private void UpdateBatch(object input)
    {
        OpenSimplexNoise noiseGen = new OpenSimplexNoise(0);
        FrameParams frameParams = (FrameParams)input;
        for (int cloudIndex = 0; cloudIndex < m_batches[frameParams.batchIndex].length; cloudIndex++)
        {
            int i = frameParams.batchIndex;
            int j = cloudIndex;

            //Calculating the noise at this cloud and time
            float x = m_batches[i].clouds[j].x * m_texScale + frameParams.time * m_timeScale;
            float y = m_batches[i].clouds[j].y * m_texScale + frameParams.time * m_timeScale;
            float noise = (float)noiseGen.Evaluate(x, y);

            //based on oise we can understand scale direction
            int dir = noise > m_minNoiseSize ? 1 : -1;

            //now calculate new scale and clamp
            float shift = m_scaleSize * frameParams.deltaTime * dir;
            float scale = m_batches[i].clouds[j].scale + shift;
            scale = Mathf.Clamp(scale, 0, m_maxScale);
            m_batches[i].clouds[j].scale = scale;

            //set new scale to object matrix
            m_batches[i].objects[j].m00 = scale;
            m_batches[i].objects[j].m11 = scale;
            m_batches[i].objects[j].m22 = scale;
        }
    }
}
