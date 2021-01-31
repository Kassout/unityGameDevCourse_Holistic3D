using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // GetKeyDown if you don't want the process to repeat until the player is pressing the key again
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(0, 300, 0);
        }
        
        // Moving the character by catching horizontal and vertical inputs
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        // change the transform to move the character
        transform.position += new Vector3(x, 0, z) * speed;
    }

    private bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo, (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }

        return false;
    }
}
