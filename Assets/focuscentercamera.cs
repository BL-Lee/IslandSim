using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class focuscentercamera : MonoBehaviour
{
    public Transform other;
    
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * 10.0f * Time.deltaTime);
        transform.LookAt(other,new Vector3(0.0f,1.0f,0.0f));
    }
}
