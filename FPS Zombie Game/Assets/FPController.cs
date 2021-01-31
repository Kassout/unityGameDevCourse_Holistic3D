using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Moving the character by catching horizontal and vertical inputs
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        // change the transform to move the character
        transform.position += new Vector3(x, 0, z) * speed;
    }
}
