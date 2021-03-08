using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    public float delay = 10;
    float destroyHeight;

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.tag == "Ragdoll")
            Invoke("StartSink", 5);
    }

    public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(transform.position) - 5;
        Collider[] colList = transform.GetComponentsInChildren<Collider>();
        foreach (Collider c in colList)
        {
            Destroy(c);
        }

        InvokeRepeating("SinkIntoGround", delay, 0.1f);
    }

    void SinkIntoGround()
    {
        transform.Translate(0, -0.001f, 0);
        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}
