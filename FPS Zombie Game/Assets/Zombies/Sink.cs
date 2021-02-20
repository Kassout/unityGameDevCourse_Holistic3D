using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Sink : MonoBehaviour
{
    private float destroyHeight;

    [SerializeField] private float delay = 10.0f;

    private void Start()
    {
        if (gameObject.CompareTag("Ragdoll"))
        {
            Invoke("StartSink", 5);
        }
    }

    public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(transform.position) - 5;
        Collider[] colList = transform.GetComponentsInChildren<Collider>();
        foreach (var col in colList)
        {
            Destroy(col);
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
