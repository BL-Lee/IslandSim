using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boidMovement : MonoBehaviour
{
    
    private Vector3 Velocity;
    private static List<GameObject> otherBoids;
    private static List<boidMovement> otherBoidScripts;
    
    public Vector3 target;
    public float targetWeight;
    private Transform bounds;
    
    public float avoidWeight;
    
    public float matchWeight;
    [Range(1, 150)]
    public float maxSpeed;
    [Range(1, 10)]
    public float maxTurnSpeed;
    [Range(1,50)]
    public float viewDist;
    [Range(0, 1)]
    public float viewAngle;

    public bool drawRays;
    
    const int numRaycasts = 10;

    public LayerMask obstacleMask;

    private static Vector3[] directions;
    // Start is called before the first frame update
    void Start()
    {
        //System.Random r = new System.Random();
        Velocity = transform.forward * 5;
        bounds = transform.parent.GetChild(0);
        avoidWeight /= 200;
        matchWeight /= 100;
    }

    public static void setOtherBoids(List<GameObject> o, List<boidMovement> b)
    {
    	otherBoids = o;
        otherBoidScripts = b;
    }

    public static void getRayAngles()
    {
        directions = new Vector3[numRaycasts];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numRaycasts; i++)
        {
            float t = (float)i / numRaycasts;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }
        
    }



    // Update is called once per frame
    void Update()
    {
        
        float obstaclePriority = ObstaclePriority();

        Velocity += AvoidAndMatch();
      
        if (obstaclePriority != 0)
        {
            Vector3 obstacle = ObstacleRule();
            {
                Velocity += obstacle / (obstaclePriority * obstaclePriority);
            }
        }

        Velocity += (target - transform.position) * targetWeight;

        Velocity = Velocity.normalized * maxSpeed;

        transform.position += Velocity * Time.deltaTime;

        transform.forward = Velocity;


        //EdgeWrap();
    }

    Vector3 AvoidAndMatch()
    {
        Vector3 avoidVector = Vector3.zero;
        Vector3 matchVector = Vector3.zero;
        for (int i = 0; i < otherBoids.Count; i++)
        {
            if (otherBoids[i] != gameObject)
            {
                float dist = Vector3.SqrMagnitude(transform.position - otherBoids[i].transform.position);
                if (dist < viewDist)
                {
                    if (angleMagnitude(otherBoids[i].transform.position) > viewAngle) 
                    {
                        avoidVector -= (otherBoids[i].transform.position - transform.position) / (dist);
                        matchVector += otherBoidScripts[i].getVelocity();                       
                    }
                }
            }
        }
        //if (avoidVector == Vector3.zero)
        //{
        	//return Vector3.MoveTowards(transform.forward, transform.right, 0.01f);
        //}

        avoidVector = avoidVector * avoidWeight;
        matchVector = matchVector * matchWeight;
        return avoidVector + matchVector;

    }
    
    float angleMagnitude(Vector3 otherPosition)
    {
        Vector3 pos = otherPosition - transform.position;
        return Vector3.Dot(transform.rotation.eulerAngles.normalized, pos.normalized);
    }
    

    float ObstaclePriority()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.SphereCast(ray,5f,out hit, viewDist * 1.5f)) { return hit.distance; }
        return 0.0f;
    }

    Vector3 ObstacleRule()
    {
        Vector3 position = transform.position;
        Vector3 outDir = Vector3.zero;
        RaycastHit hit;
        float maxDist = -1.0f;
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(directions[i]);
            
            if (Physics.SphereCast(new Ray(position,dir),5f, out hit, viewDist))
            {
                if (hit.distance > maxDist)
                {
                	maxDist = hit.distance;
                	outDir = dir;
                }
            }
            else 
            {
            	return dir;
            }
        }
        return outDir;
    }
   
    

    void EdgeWrap()
    {
        Vector3 Position = transform.position;
        if (Position.x > bounds.localScale.x / 2) { transform.position = new Vector3(-bounds.localScale.x / 2, Position.y, Position.z); }
        else if (Position.x < -bounds.localScale.x / 2) { transform.position = new Vector3(bounds.localScale.x / 2, Position.y, Position.z); }
        if (Position.y > bounds.localScale.y / 2) { transform.position = new Vector3(Position.x, -bounds.localScale.y / 2, Position.z); }
        else if (Position.y < -bounds.localScale.y / 2) { transform.position = new Vector3(Position.x, bounds.localScale.y / 2, Position.z); }
        if (Position.z > bounds.localScale.z / 2) { transform.position = new Vector3(Position.x, Position.y, -bounds.localScale.z / 2); }
        else if (Position.z < -bounds.localScale.z / 2) { transform.position = new Vector3(Position.x, Position.y, bounds.localScale.z / 2); }

    }

    public Vector3 getVelocity() { return Velocity; }
}
