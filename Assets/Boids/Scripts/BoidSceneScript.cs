using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSceneScript : MonoBehaviour
{
    public GameObject Boid;
    public int numberOfBoids;
    public Transform boundingBox;
    
    private List<GameObject> otherBoids;
    private List<boidMovement> otherBoidScripts;
    // Start is called before the first frame update
    void Start()
    {
     
        otherBoids = new List<GameObject>();
        otherBoidScripts = new List<boidMovement>();

        for (int j = 0; j < gameObject.transform.childCount; j++)
        {
            if (gameObject.transform.GetChild(j).TryGetComponent(out boidMovement b))
            {
                otherBoids.Add(gameObject.transform.GetChild(j).gameObject);
                gameObject.transform.GetChild(j).parent = gameObject.transform;
                otherBoidScripts.Add(b);
            }
        }

        for (int i = 0; i < numberOfBoids; i++)
        {
            
            GameObject boid = Instantiate(Boid,
                Vector3.zero,
                Quaternion.identity);
                
          
            boid.transform.forward = new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), Random.Range(-30f, -30f));
            otherBoids.Add(boid);
            otherBoidScripts.Add(boid.GetComponent<boidMovement>());
            boid.transform.parent = gameObject.transform;
           
        }

        boidMovement.setOtherBoids(otherBoids, otherBoidScripts);
        boidMovement.getRayAngles();
    }
 
    
}
