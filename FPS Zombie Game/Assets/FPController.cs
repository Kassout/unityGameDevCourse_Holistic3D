using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FPController : MonoBehaviour
{
    public GameObject cam;
    public Animator anim;
    public AudioSource[] footSteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickup;
    public AudioSource medKitPickup;
    public AudioSource triggerSound;
    public AudioSource deathSound;
    public AudioSource reloadWeapon;

    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float xSensitivity = 2f;
    [SerializeField] private float ySensitivity = 2f;
    [SerializeField] private float minimumX = -90f;
    [SerializeField] private float maximumX = 90f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Quaternion cameraRotation;
    private Quaternion characterRotation;
    private bool cursorIsLocked = true;
    private bool lockCursor = true;

    private float x;
    private float z;

    private bool playingWalking = false;
    private bool previouslyGrounded = true;

    #region Inventory

    private int ammo = 0;
    private int maxAmmo = 50;

    private int ammoClip = 0;
    private int ammoClipMax = 10;
    
    private int health = 0;
    private int maxHealth = 100;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        cameraRotation = cam.transform.localRotation;
        characterRotation = transform.localRotation;

        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            anim.SetBool("arm", !anim.GetBool("arm"));
        }
        
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("fire")) 
        {
            if (ammoClip > 0)
            {
                anim.SetTrigger("fire");
                ammoClip--;
            }
            else if (anim.GetBool("arm"))
            {
                triggerSound.Play();
            }
            Debug.Log("Ammo Left in Clip: " + ammoClip);
        }

        if (Input.GetKeyDown(KeyCode.R) && anim.GetBool("arm"))
        {
            anim.SetTrigger("reload");
            reloadWeapon.Play();
            int amountNeeded = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeeded < ammo ? amountNeeded : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable;
            
            Debug.Log("Ammo Left: " + ammo);
            Debug.Log("Ammo in Clip: " + ammoClip);
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool("walk"))
            {
                anim.SetBool("walk", true);
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
        else if (anim.GetBool("walk"))
        {
            anim.SetBool("walk", false);
            CancelInvoke("PlayFootStepAudio");
            playingWalking = false;
        }

        bool grounded = IsGrounded();
        // GetKeyDown if you don't want the process to repeat until the player is pressing the key again
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool("walk"))
            {
                CancelInvoke("PlayFootStepAudio");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }
        previouslyGrounded = grounded;
    }

    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footSteps.Length);

        audioSource = footSteps[n];
        audioSource.Play();
        footSteps[n] = footSteps[0];
        footSteps[0] = audioSource;

        playingWalking = true;
    }
    
    private void FixedUpdate()
    {
        // Rotating the character and the camera around X and Y axis
        float yRotation = Input.GetAxis("Mouse X") * ySensitivity;
        float xRotation = Input.GetAxis("Mouse Y") * xSensitivity;

        cameraRotation *= Quaternion.Euler(-xRotation, 0, 0);
        characterRotation *= Quaternion.Euler(0, yRotation, 0);

        cameraRotation = ClampRotationAroundXAxis(cameraRotation);

        transform.localRotation = characterRotation;
        cam.transform.localRotation = cameraRotation;

        // Moving the character by catching horizontal and vertical inputs
        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;

        // Change the transform to move the character
        // Camera forward-facing movement
        transform.position += cam.transform.forward * z + cam.transform.right * x; //new Vector3(x, 0, z) * speed;
        
        // Trigger the cursor locking feature
        UpdateCursorLock();
    }
    
    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        // Normalize our current quaternion
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        // Converting quaternion value into a Euler value (rotation around the X axis angle)
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minimumX, maximumX);
        
        // Converting Euler angle value into a quaternion value
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
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

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ammo") && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            Debug.Log("Ammo:" + ammo);
            Destroy(other.gameObject);
            ammoPickup.Play();
        }
        else if (other.gameObject.CompareTag("MedKit") && health < maxHealth)
        {
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            Debug.Log("MedKit");
            Destroy(other.gameObject); 
            medKitPickup.Play();
        } 
        else if (other.gameObject.CompareTag("Lava"))
        {
            health = Mathf.Clamp(health - 50, 0, maxHealth);
            Debug.Log("Health level: " + health);
            if (health <= 0)
            {
                deathSound.Play();
            }
        }
        else if (IsGrounded())
        {
            if (anim.GetBool("walk") && !playingWalking)
            {
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);   
            }
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
        {
            InternalLockUpdate();
        }
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        } else if (Input.GetMouseButtonUp(0))
        {
            cursorIsLocked = true;
        }
        
        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
