using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVisual : MonoBehaviour
{
    public GameObject prefab;
    public bool DoDestroyVisuals;
    public void showNoise(int[,,] noiseMap)
    {
        if (!DoDestroyVisuals)
        {
            foreach (GameObject i in GameObject.FindGameObjectsWithTag("noiseVisuals"))
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(i);
                };
            }
            int xlength = noiseMap.GetLength(0);
            int ylength = noiseMap.GetLength(1);
            int zlength = noiseMap.GetLength(2);

            for (int x = 0; x < xlength; x++)
            {
                for (int y = 0; y < ylength; y++)
                {
                    for (int z = 0; z < zlength; z++)
                    {
                        GameObject temp = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                        Debug.Log(noiseMap[x, y, z]);
                        if (noiseMap[x, y, z] == 1)
                        {
                            temp.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
                        }
                        else
                        {
                            temp.GetComponent<MeshRenderer>().material.color = new Color(0, 1,0);
                        }
                        temp.tag = "noiseVisuals";

                    }
                }
            }
        }
    }
    public void destroyVisuals()
    {
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("noiseVisuals"))
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(i);
            };
        }
    }
}
